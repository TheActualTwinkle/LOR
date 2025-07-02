using DatabaseApp.Application.Classes.Command;
using DatabaseApp.Application.Groups.Command.CreateGroup;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Shared.GrpcServices;

namespace DatabaseApp.WebApi.GrpcServices;

public class GrpcDatabaseUpdaterService(ISender mediator, ILogger<GrpcDatabaseUpdaterService> logger) : DatabaseUpdater.DatabaseUpdaterBase
{
    public override async Task<Empty> SetAvailableGroups(SetAvailableGroupsRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new CreateGroupsCommand
            {
                GroupNames = request.GroupNames.ToList()
            }, context.CancellationToken);

        if (result.IsFailed)
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.First().Message));

        return new Empty();
    }

    public override async Task<Empty> SetAvailableClasses(SetAvailableClassesRequest request, ServerCallContext context)
    {
        logger.LogInformation("Received request to set available classes for group: {GroupName}", request.GroupName);
        
        var createClassesResult = await mediator.Send(
            new CreateClassesCommand
            {
                GroupName = request.GroupName,
                Classes = request.Classes.ToDictionary(
                    c => c.Key,
                    c => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(c.Value).DateTime))
            },
            context.CancellationToken);

        if (createClassesResult.IsFailed)
            throw new RpcException(new Status(StatusCode.Internal, createClassesResult.Errors.First().Message));
        
        logger.LogInformation("Successfully set available classes for group: {GroupName}", request.GroupName);

        return new Empty();
    }
}