using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Class.Queries.GetOutdatedClasses;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Group.Queries.GetGroup;
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

    public override async Task<Empty> SetAvailableLabClasses(SetAvailableLabClassesRequest request, ServerCallContext context)
    {
        Result<List<ClassDto>> oldClasses = await mediator.Send(new GetClassesQuery
        {
            GroupName = request.GroupName
        });
        
        if (oldClasses.IsFailed) return new Empty();
        
        try
        {
            await mediator.Send(new CreateClassesCommand
            {
                GroupName = request.GroupName,
                Classes = request.Classes.ToDictionary(
                    c => c.Key,
                    c => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(c.Value).DateTime)
                )
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        Result<List<int>> outdatedClassList = await mediator.Send(new GetOutdatedClassesQuery());

        if (outdatedClassList.IsSuccess && outdatedClassList.Value.Count != 0)
        {
            await mediator.Send(new DeleteQueueCommand
            {
                OutdatedClassList = outdatedClassList.Value
            });
        
            await mediator.Send(new DeleteClassCommand
            {
                ClassesId = outdatedClassList.Value
            });
        }
        
        Result<List<ClassDto>> classes = await mediator.Send(new GetClassesQuery
        {
            GroupName = request.GroupName
        });
        
        if (classes.IsFailed) return new Empty();
        
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupName, classes.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI

        List<ClassDto> newClasses = classes.Value.Except(oldClasses.Value).ToList();
        
        if (newClasses.Count == 0) return new Empty();
        
        Result<GroupDto> groupDto = await mediator.Send(new GetGroupQuery
        {
            GroupName = request.GroupName
        });
        
        if (classes.IsFailed) return new Empty();
        
        NewClassesMessage newClassesMessage = new()
        {
            GroupId = groupDto.Value.Id,
            Classes = newClasses.Select(x => new Class { Id = x.Id, Name = x.Name, Date = x.Date })
        };
        await bus.Publish(newClassesMessage, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI
        
        return new Empty();
    }
}