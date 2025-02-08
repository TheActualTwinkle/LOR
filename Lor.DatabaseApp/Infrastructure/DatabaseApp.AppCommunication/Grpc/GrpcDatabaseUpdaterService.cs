using DatabaseApp.AppCommunication.Consumers.Data;
using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.CreateClasses;
using DatabaseApp.Application.Class.Command.DeleteClasses;
using DatabaseApp.Application.Class.Queries;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MassTransit;
using MediatR;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService(
    IBus bus,
    ICacheService cacheService, 
    ISender mediator
    ) 
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
        // Snapshot of classes BEFORE new has been added.
        var oldClasses = await mediator.Send(new GetClassesQuery
        {
            GroupName = request.GroupName
            
        }, context.CancellationToken);
        
        if (oldClasses.IsFailed)
            throw new RpcException(new Status(StatusCode.NotFound, oldClasses.Errors.First().Message));

        var createClassesResult = await CreateClasses(request.Classes, request.GroupName, context.CancellationToken);
        
        if (createClassesResult.IsFailed)
            throw new RpcException(new Status(StatusCode.Internal, createClassesResult.Errors.First().Message));

        // TODO: Has to be moved outside the SetAvailableClasses logic.
        await DeleteOutdatedClasses(context.CancellationToken);

        var classes = await mediator.Send(new GetClassesQuery
        {
            GroupName = request.GroupName
        }, context.CancellationToken);

        if (classes.IsFailed)
            throw new RpcException(new Status(StatusCode.NotFound, classes.Errors.First().Message));

        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupName, classes.Value, cancellationToken: context.CancellationToken);

        var newClasses = classes.Value.Except(oldClasses.Value).OrderBy(x => x.Id).ToList();

        if (newClasses.Count == 0) return new Empty();

        await PublishNewClassesMessage(request.GroupName, newClasses, context.CancellationToken);

        return new Empty();
    }

    private async Task<Result> CreateClasses(
        IDictionary<string, long> classesDate,
        string groupName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await mediator.Send(new CreateClassesCommand
            {
                GroupName = groupName,
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

            await mediator.Send(new DeleteClassesCommand
            {
                ClassesId = outdatedClassList.Value
            }, cancellationToken);
        }
    }

    private async Task PublishNewClassesMessage(string groupName, IEnumerable<ClassDto> newClasses, CancellationToken cancellationToken = default)
    {
        NewClassesMessage newClassesMessage = new()
        {
            GroupName = groupName,
            Classes = newClasses.Select(x => new Class { Name = x.Name, Date = x.Date })
        };

        await bus.Publish(newClassesMessage, cancellationToken);
    }
}