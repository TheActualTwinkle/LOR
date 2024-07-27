using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Common.Converters;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Queries.GetGroups;
using DatabaseApp.Application.Queue;
using DatabaseApp.Application.Queue.Commands.CreateQueue;
using DatabaseApp.Application.Queue.Queries.GetQueue;
using DatabaseApp.Application.User;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Application.User.Queries.GetUserGroup;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseService(IUnitOfWork unitOfWork) : Database.DatabaseBase
{
    public override async Task<GetUserGroupReply> GetUserGroup(GetUserGroupRequest request, ServerCallContext context)
    {
        GetUserGroupQuery getUserGroupQuery = new() { TelegramId = request.UserId };
        GetUserGroupQueryHandler getUserGroupQueryHandler = new GetUserGroupQueryHandler(unitOfWork);

        Result<UserDto> userDto = await getUserGroupQueryHandler.Handle(getUserGroupQuery,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (userDto.IsFailed)
            return await Task.FromResult(new GetUserGroupReply
                { IsFailed = true, ErrorMessage = userDto.Errors.ToString() });

        return await Task.FromResult(new GetUserGroupReply { GroupName = userDto.Value.GroupName });
    }

    public override async Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        GetGroupsQueryHandler getGroupsQueryHandler = new GetGroupsQueryHandler(unitOfWork);

        Result<GroupDto> groupDto = await getGroupsQueryHandler.Handle(new EmptyRequest(),
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        GetAvailableGroupsReply reply = new();
        
        await reply.IdGroupsMap.FromDictionary(groupDto.Value.GroupList);

        return await Task.FromResult(reply);
    }

    public override async Task<GetAvailableLabClassesReply> GetAvailableLabClasses(
        GetAvailableLabClassesRequest request, ServerCallContext context)
    {
        GetClassesQuery getClassesQuery = new GetClassesQuery { TelegramId = request.UserId };
        GetClassesQueryHandler getClassesQueryHandler = new GetClassesQueryHandler(unitOfWork);

        Result<ClassDto> classDto = await getClassesQueryHandler.Handle(getClassesQuery,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (classDto.IsFailed)
            return await Task.FromResult(new GetAvailableLabClassesReply
                { IsFailed = true, ErrorMessage = classDto.Errors.ToString() });

        GetAvailableLabClassesReply reply = new();

        await reply.IdClassMap.FromDictionary(classDto.Value.ClassList);

        return await Task.FromResult(reply);
    }

    public override async Task<TrySetGroupReply> TrySetGroup(TrySetGroupRequest request, ServerCallContext context)
    {
        CreateUserCommand
            createUserCommand = new()
                { TelegramId = request.UserId, GroupName = request.GroupName }; //TODO: добавить в реквест фулнейм
        CreateUserCommandHandler createUserCommandHandler = new CreateUserCommandHandler(unitOfWork);

        Result result = await createUserCommandHandler.Handle(createUserCommand,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (result.IsFailed)
            return await Task.FromResult(new TrySetGroupReply
                { IsFailed = true, ErrorMessage = result.Errors.ToString() });

        GetUserGroupQuery getUserGroupQuery = new() { TelegramId = request.UserId };
        GetUserGroupQueryHandler getUserGroupQueryHandler = new GetUserGroupQueryHandler(unitOfWork);

        Result<UserDto> userDto = await getUserGroupQueryHandler.Handle(getUserGroupQuery,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        return await Task.FromResult(new TrySetGroupReply { GroupName = userDto.Value.GroupName });
    }

    public override async Task<TryEnqueueInClassReply> TryEnqueueInClass(TryEnqueueInClassRequest request,
        ServerCallContext context)
    {
        CreateQueueCommand createQueueCommand = new CreateQueueCommand { TelegramId = request.UserId, ClassId = request.ClassId };
        CreateQueueCommandHandler createQueueCommandHandler = new CreateQueueCommandHandler(unitOfWork);

        Result result = await createQueueCommandHandler.Handle(createQueueCommand,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (result.IsFailed)
            return await Task.FromResult(new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = result.Errors.ToString() });

        GetQueueQuery getQueueQuery = new GetQueueQuery { TelegramId = request.UserId, ClassId = request.ClassId };
        GetQueueQueryHandler getQueueQueryHandler = new GetQueueQueryHandler(unitOfWork);

        Result<QueueDto> queueDto = await getQueueQueryHandler.Handle(getQueueQuery, new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        TryEnqueueInClassReply reply = new();

        await reply.StudentsQueue.FromList(queueDto.Value.QueueList);

        return await Task.FromResult(reply);
    }
}