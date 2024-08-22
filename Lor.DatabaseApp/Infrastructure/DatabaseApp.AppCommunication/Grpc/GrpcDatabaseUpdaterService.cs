﻿using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Class.Queries.GetOutdatedClasses;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Group.Queries.GetGroups;
using DatabaseApp.Application.Queue.Commands.DeleteQueue;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MassTransit;
using MediatR;
using TelegramBotApp.AppCommunication.Consumers.Data;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService(ISender mediator, ICacheService cacheService, IBus bus) : DatabaseUpdater.DatabaseUpdaterBase
{
    public override async Task<Empty> SetAvailableGroups(SetAvailableGroupsRequest request, ServerCallContext context)
    {
        foreach (string? groupName in request.GroupNames.ToList())
        {
            await mediator.Send(new CreateGroupCommand
            {
                GroupName = groupName
            });
        }
        
        Result<List<GroupDto>> groups = await mediator.Send(new GetGroupsQuery());
        
        await cacheService.SetAsync(Constants.AvailableGroupsKey, groups.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI
        
        return new Empty();
    }

    public override async Task<Empty> SetAvailableLabClasses(SetAvailableLabClassesRequest request,
        ServerCallContext context)
    {
        foreach (KeyValuePair<string, long> classObject in request.Classes)
        {
            DateOnly date;
            try
            {
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(classObject.Value).DateTime;
                date = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }

            // TODO @ext4zzzy: Может оптимизировать, чтобы не делать запрос на каждый класс, а слать сразу все?
            await mediator.Send(new CreateClassCommand
            {
                GroupName = request.GroupName,
                ClassName = classObject.Key,
                Date = date
            });
        }

        NewClassesMessage message = new() { Classes = [new Class { Id = 1, Name = "kek", Date = new DateOnly()}] }; // TODO: REMOVE AFTER TESTING
        await bus.Publish(message, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: REMOVE AFTER TESTING
        
        Result<List<int>> outdatedClassList = await mediator.Send(new GetOutdatedClassesQuery());

        // TODO @ext4zzzy: Кэш не обновляется, если список устаревших классов пуст или фейл.
        if (outdatedClassList.IsFailed || outdatedClassList.Value.Count == 0) return new Empty();
        
        await mediator.Send(new DeleteQueueCommand
        {
            OutdatedClassList = outdatedClassList.Value
        });
        
        await mediator.Send(new DeleteClassCommand
        {
            OutdatedClassList = outdatedClassList.Value
        });

        Result<List<ClassDto>> classes = await mediator.Send(new GetClassesQuery
        {
            GroupName = request.GroupName
        });
        
        if (classes.IsFailed) return new Empty();
        
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupName, classes.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI
        
        // TODO: send ONLY new classes.
        // await bus.Publish(classes.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI
        
        return new Empty();
    }
}