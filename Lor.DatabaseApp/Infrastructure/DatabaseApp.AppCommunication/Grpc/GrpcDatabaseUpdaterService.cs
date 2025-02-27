using DatabaseApp.AppCommunication.Messages;
using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command;
using DatabaseApp.Application.Class.Queries;
using DatabaseApp.Application.Group.Command.CreateGroup;
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
    ISender mediator) 
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
    
    private async Task PublishNewClassesMessage(string groupName, IEnumerable<ClassDto> newClasses, CancellationToken cancellationToken = default)
    {
        NewClassesMessage newClassesMessage = new()
        {
            GroupName = groupName,
            Classes = newClasses.Select(c => new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Date = c.Date
            })
        };

        await bus.Publish(newClassesMessage, cancellationToken);
    }
}