using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Class.Queries.GetOutdatedClasses;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Group.Queries.GetGroupInfo;
using DatabaseApp.Application.Group.Queries.GetGroups;
using DatabaseApp.Application.Queue.Commands.DeleteQueue;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService(ISender mediator, ICacheService cacheService) : DatabaseUpdater.DatabaseUpdaterBase
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
        
        await cacheService.SetAsync(Constants.AvailableGroupsKey, groups.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
        
        return new Empty();
    }

    public override async Task<Empty> SetAvailableLabClasses(SetAvailableLabClassesRequest request,
        ServerCallContext context)
    {
        List<ClassDto> classDto = [];
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
            
            classDto.Add(new ClassDto
            {
                Date = date,
                Name = classObject.Key
            });
        }
        
        Result<List<int>> outdatedClassList = await mediator.Send(new GetOutdatedClassesQuery());

        if (outdatedClassList.IsFailed) return new Empty();
        
        if (outdatedClassList.Value.Count == 0) return new Empty();
        
        await mediator.Send(new DeleteQueueCommand
        {
            OutdatedClassList = outdatedClassList.Value
        });
        
        await mediator.Send(new DeleteClassCommand
        {
            OutdatedClassList = outdatedClassList.Value
        });
        
        Result<GroupDto> group = await mediator.Send(new GetGroupInfoQuery
        {
            GroupName = request.GroupName
        });
        
        // TODO: get List<Dto>
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + group.Value.Id, newDto, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
        
        return new Empty();
    }
}