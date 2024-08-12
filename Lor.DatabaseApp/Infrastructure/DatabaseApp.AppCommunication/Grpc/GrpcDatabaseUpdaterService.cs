using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Class.Queries.GetOutdatedClasses;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Queue.Commands.DeleteQueue;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService(ISender mediator) : DatabaseUpdater.DatabaseUpdaterBase
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
        
        return await Task.FromResult(new Empty());
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

            await mediator.Send(new CreateClassCommand
            {
                GroupName = request.GroupName,
                ClassName = classObject.Key,
                Date = date
            });
        }

        Result<List<ClassDto>> outdatedClassList = await mediator.Send(new GetOutdatedClassesQuery());

        if (outdatedClassList.IsFailed) return await Task.FromResult(new Empty());

        await mediator.Send(new DeleteClassCommand
        {
            OutdatedClassList = outdatedClassList.Value
        });

        await mediator.Send(new DeleteQueueCommand
        {
            OutdatedClassList = outdatedClassList.Value
        });

        return await Task.FromResult(new Empty());
    }
}