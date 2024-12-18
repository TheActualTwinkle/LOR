﻿using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Class.Queries.GetOutdatedClasses;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Group.Queries.GetGroup;
using DatabaseApp.Application.Queue.Commands.DeleteOutdatedQueues;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using TelegramBotApp.AppCommunication.Consumers.Data;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService(ISender mediator, ICacheService cacheService, IBus bus, ILogger<GrpcDatabaseUpdaterService> logger) : DatabaseUpdater.DatabaseUpdaterBase
{
    public override async Task<Empty> SetAvailableGroups(SetAvailableGroupsRequest request, ServerCallContext context)
    {
        await mediator.Send(new CreateGroupsCommand
        {
            GroupNames = request.GroupNames.ToList()
        }, context.CancellationToken);
        
        return new Empty();
    }

    public override async Task<Empty> SetAvailableClasses(SetAvailableClassesRequest request, ServerCallContext context)
    {
        var getGroupResult = await mediator.Send(new GetGroupQuery
        {
            GroupName = request.GroupName
        }, context.CancellationToken);

        if (getGroupResult.IsFailed) return new Empty();
        
        var oldClasses = await mediator.Send(new GetClassesQuery
        {
            GroupId = getGroupResult.Value.Id
        }, context.CancellationToken);
        
        if (oldClasses.IsFailed) return new Empty();
        
        try
        {
            await mediator.Send(new CreateClassesCommand
            {
                GroupId = getGroupResult.Value.Id,
                Classes = request.Classes.ToDictionary(
                    c => c.Key,
                    c => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(c.Value).DateTime)
                )
            }, context.CancellationToken);
        }
        catch (Exception e)
        {
            logger.LogCritical("Fatal on CreateClassesCommand: {message}", e.Message);
            throw;
        }
        
        var outdatedClassList = await mediator.Send(new GetOutdatedClassesQuery());

        if (outdatedClassList.IsSuccess && outdatedClassList.Value.Count != 0)
        {
            await mediator.Send(new DeleteQueuesForClassesCommand
            {
                ClassesId = outdatedClassList.Value
            }, context.CancellationToken);
        
            await mediator.Send(new DeleteClassCommand
            {
                ClassesId = outdatedClassList.Value
            }, context.CancellationToken);
        }
        
        var classes = await mediator.Send(new GetClassesQuery
        {
            GroupId = getGroupResult.Value.Id
        }, context.CancellationToken);
        
        if (classes.IsFailed) return new Empty();
        
        var groupDto = await mediator.Send(new GetGroupQuery
        {
            GroupName = request.GroupName
        });
        
        if (classes.IsFailed) return new Empty();
        
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + groupDto.Value.Id, classes.Value, cancellationToken: context.CancellationToken);

        var newClasses = classes.Value.Except(oldClasses.Value).ToList();
        
        if (newClasses.Count == 0) return new Empty();
        
        NewClassesMessage newClassesMessage = new()
        {
            GroupId = groupDto.Value.Id,
            Classes = newClasses.Select(x => new Class { Id = x.Id, Name = x.Name, Date = x.Date })
        };
        
        await bus.Publish(newClassesMessage, cancellationToken: context.CancellationToken);
        
        return new Empty();
    }
}