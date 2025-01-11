using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Class.Queries.GetOutdatedClasses;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Group.Queries.GetGroup;
using DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using TelegramBotApp.AppCommunication.Consumers.Data;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService(
    ISender mediator,
    ICacheService cacheService,
    IBus bus,
    ILogger<GrpcDatabaseUpdaterService> logger) 
    : DatabaseUpdater.DatabaseUpdaterBase
{
    public override async Task<Empty> SetAvailableGroups(SetAvailableGroupsRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(new CreateGroupsCommand
        {
            GroupNames = request.GroupNames.ToList()
        }, context.CancellationToken);

        if (result.IsFailed)
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.First().Message));

        return new Empty();
    }

    public override async Task<Empty> SetAvailableClasses(SetAvailableClassesRequest request, ServerCallContext context)
    {
        var getGroupResult = await mediator.Send(new GetGroupQuery
        {
            GroupName = request.GroupName
        }, context.CancellationToken);

        if (getGroupResult.IsFailed)
            throw new RpcException(new Status(StatusCode.NotFound, getGroupResult.Errors.First().Message));

        // Snapshot of classes BEFORE new has been added.
        var oldClasses = await mediator.Send(new GetClassesQuery
        {
            GroupId = getGroupResult.Value.Id
        }, context.CancellationToken);
        
        if (oldClasses.IsFailed)
            throw new RpcException(new Status(StatusCode.NotFound, oldClasses.Errors.First().Message));

        var createClassesResult = await CreateClasses(request.Classes, getGroupResult.Value.Id, context.CancellationToken);
        
        if (createClassesResult.IsFailed)
            throw new RpcException(new Status(StatusCode.Internal, createClassesResult.Errors.First().Message));

        // TODO: Has to be moved outside the SetAvailableClasses logic.
        await DeleteOutdatedClasses(context.CancellationToken);

        var classes = await mediator.Send(new GetClassesQuery
        {
            GroupId = getGroupResult.Value.Id
        }, context.CancellationToken);

        if (classes.IsFailed)
            throw new RpcException(new Status(StatusCode.NotFound, classes.Errors.First().Message));

        var groupDto = await mediator.Send(new GetGroupQuery
        {
            GroupName = request.GroupName
        });
        
        if (groupDto.IsFailed)
            throw new RpcException(new Status(StatusCode.NotFound, groupDto.Errors.First().Message));

        await cacheService.SetAsync(Constants.AvailableClassesPrefix + groupDto.Value.Id, classes.Value, cancellationToken: context.CancellationToken);

        var newClasses = classes.Value.Except(oldClasses.Value).OrderBy(x => x.Id).ToList();

        if (newClasses.Count == 0) return new Empty();

        await PublishNewClassesMessage(groupDto.Value, newClasses, context.CancellationToken);

        return new Empty();
    }

    private async Task<Result> CreateClasses(
        IDictionary<string, long> classesDate,
        int groupId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await mediator.Send(new CreateClassesCommand
            {
                GroupId = groupId,
                Classes = classesDate.ToDictionary(
                    c => c.Key,
                    c => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(c.Value).DateTime)
                )
            }, cancellationToken);
        }
        catch (Exception e)
        {
            return Result.Fail($"Error while creating classes {e.Message}");
        }
    }

    private async Task DeleteOutdatedClasses(CancellationToken cancellationToken = default)
    {
        var outdatedClassList = await mediator.Send(new GetOutdatedClassesQuery(), cancellationToken);

        if (outdatedClassList.IsSuccess &&
            outdatedClassList.Value.Count != 0)
        {
            await mediator.Send(new DeleteQueuesForClassesCommand
            {
                ClassesId = outdatedClassList.Value
            }, cancellationToken);

            await mediator.Send(new DeleteClassCommand
            {
                ClassesId = outdatedClassList.Value
            }, cancellationToken);
        }
    }

    private async Task PublishNewClassesMessage(GroupDto groupDto, IEnumerable<ClassDto> newClasses, CancellationToken cancellationToken = default)
    {
        NewClassesMessage newClassesMessage = new()
        {
            GroupId = groupDto.Id,
            Classes = newClasses.Select(x => new Class { Name = x.Name, Date = x.Date })
        };

        await bus.Publish(newClassesMessage, cancellationToken);
    }
}