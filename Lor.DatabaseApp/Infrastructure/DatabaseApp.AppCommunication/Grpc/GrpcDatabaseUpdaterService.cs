using DatabaseApp.Application.Class.Command.CreateClass;
using DatabaseApp.Application.Class.Command.DeleteClass;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Application.Queue.Commands.DeleteQueue;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService : DatabaseUpdater.DatabaseUpdaterBase
{
    private IUnitOfWork _unitOfWork;

    public override async Task<Empty> SetAvailableGroups(SetAvailableGroupsRequest request, ServerCallContext context)
    {
        foreach (var groupName in request.GroupNames.ToList())
        {
            CreateGroupCommand createGroupCommand = new() { GroupName = groupName };
            CreateGroupCommandHandler createGroupCommandHandler = new CreateGroupCommandHandler(_unitOfWork);

            await createGroupCommandHandler.Handle(createGroupCommand,
                new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
        }

        return await Task.FromResult(new Empty());
    }

    public override async Task<Empty> SetAvailableLabClasses(SetAvailableLabClassesRequest request,
        ServerCallContext context)
    {
        foreach (var className in request.ClassNames.ToList())
        {
            CreateClassCommand createClassCommand = new() { GroupName = request.GroupName, ClassName = className /*Date = request.*/ }; //TODO: добавить в реквест дату лабы
            CreateClassCommandHandler createClassCommandHandler = new CreateClassCommandHandler(_unitOfWork);

            await createClassCommandHandler.Handle(createClassCommand,
                new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
        }

        List<Class>? classList =
            await _unitOfWork.ClassRepository.GetOutdatedClasses(new CancellationTokenSource(TimeSpan.FromSeconds(10))
                .Token);
        
        if (classList is null) return new Empty();

        DeleteClassCommand deleteClassCommand = new() { OutdatedClassList = classList };
        DeleteClassCommandHandler deleteClassCommandHandler = new DeleteClassCommandHandler(_unitOfWork);

        await deleteClassCommandHandler.Handle(deleteClassCommand,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        DeleteQueueCommand deleteQueueCommand = new() { OutdatedClaasList = classList };
        DeleteQueueCommandHandler deleteQueueCommandHandler = new DeleteQueueCommandHandler(_unitOfWork);

        await deleteQueueCommandHandler.Handle(deleteQueueCommand,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        return await Task.FromResult(new Empty());
    }
}