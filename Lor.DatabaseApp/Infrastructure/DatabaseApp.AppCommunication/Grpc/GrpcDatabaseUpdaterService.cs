using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Queue.Commands.DeleteQueue;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService(IUnitOfWork unitOfWork) : DatabaseUpdater.DatabaseUpdaterBase
{
    public override async Task<Empty> SetAvailableGroups(SetAvailableGroupsRequest request, ServerCallContext context)
    {
        foreach (string? groupName in request.GroupNames.ToList())
        {
            CreateGroupCommand createGroupCommand = new() { GroupName = groupName };
            CreateGroupCommandHandler createGroupCommandHandler = new(unitOfWork);

            await createGroupCommandHandler.Handle(createGroupCommand,
                new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
        }

        return await Task.FromResult(new Empty());
    }

    public override async Task<Empty> SetAvailableLabClasses(SetAvailableLabClassesRequest request,
        ServerCallContext context)
    {
        foreach (KeyValuePair<string, Timestamp> classObject in request.Classes)
        {
            DateOnly date;
            try
            {
                DateTime dateTime = classObject.Value.ToDateTime();
                date = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }
            
            CreateClassCommand createClassCommand = new() { GroupName = request.GroupName, ClassName = classObject.Key, Date = date };
            CreateClassCommandHandler createClassCommandHandler = new(unitOfWork);

            await createClassCommandHandler.Handle(createClassCommand,
                new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
        }

        List<Class>? classList =
            await unitOfWork.ClassRepository.GetOutdatedClasses(new CancellationTokenSource(TimeSpan.FromSeconds(10))
                .Token);
        
        if (classList is null) return new Empty();

        DeleteClassCommand deleteClassCommand = new() { OutdatedClassList = classList };
        DeleteClassCommandHandler deleteClassCommandHandler = new(unitOfWork);

        await deleteClassCommandHandler.Handle(deleteClassCommand,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        DeleteQueueCommand deleteQueueCommand = new() { OutdatedClaasList = classList };
        DeleteQueueCommandHandler deleteQueueCommandHandler = new(unitOfWork);

        await deleteQueueCommandHandler.Handle(deleteQueueCommand,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        return await Task.FromResult(new Empty());
    }
}