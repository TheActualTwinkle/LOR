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

public class GrpcDatabaseService : Database.DatabaseBase
{
    private IUnitOfWork _unitOfWork;
    private ClassDto _classDto;
    private GroupDto _groupDto;
    private QueueDto _queueDto;
    private UserDto _userDto;

    public override async Task<GetUserGroupReply> GetUserGroup(GetUserGroupRequest request, ServerCallContext context)
    {
        GetUserGroupQuery getUserGroupQuery = new() { TelegramId = request.UserId };
        GetUserGroupQueryHandler getUserGroupQueryHandler = new GetUserGroupQueryHandler(_unitOfWork, _userDto);

        Result response = await getUserGroupQueryHandler.Handle(getUserGroupQuery,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (response.IsFailed)
            return await Task.FromResult(new GetUserGroupReply
                { IsFailed = true, ErrorMessage = response.Errors.ToString() });

        return await Task.FromResult(new GetUserGroupReply { GroupName = _userDto.GroupName });
    }

    public override async Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        GetGroupsQueryHandler getGroupsQueryHandler = new GetGroupsQueryHandler(_unitOfWork, _groupDto);

        await getGroupsQueryHandler.Handle(new EmptyRequest(),
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        GetAvailableGroupsReply reply = new();

        await reply.IdGroupsMap.FromDictionary(_groupDto.GroupList);

        return await Task.FromResult(reply);
    }

    public override async Task<GetAvailableLabClassesReply> GetAvailableLabClasses(
        GetAvailableLabClassesRequest request, ServerCallContext context)
    {
        GetClassesQuery getClassesQuery = new GetClassesQuery { TelegramId = request.UserId };
        GetClassesQueryHandler getClassesQueryHandler = new GetClassesQueryHandler(_unitOfWork, _classDto);

        Result response = await getClassesQueryHandler.Handle(getClassesQuery,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (response.IsFailed)
            return await Task.FromResult(new GetAvailableLabClassesReply
                { IsFailed = true, ErrorMessage = response.Errors.ToString() });

        GetAvailableLabClassesReply reply = new();

        await reply.IdClassMap.FromDictionary(_classDto.ClassList);

        return await Task.FromResult(reply);
    }

    public override async Task<TrySetGroupReply> TrySetGroup(TrySetGroupRequest request, ServerCallContext context)
    {
        CreateUserCommand
            createUserCommand = new()
                { TelegramId = request.UserId, GroupName = request.GroupName }; //TODO: добавить в реквест фулнейм
        CreateUserCommandHandler createUserCommandHandler = new CreateUserCommandHandler(_unitOfWork);

        Result response = await createUserCommandHandler.Handle(createUserCommand,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (response.IsFailed)
            return await Task.FromResult(new TrySetGroupReply
                { IsFailed = true, ErrorMessage = response.Errors.ToString() });

        GetUserGroupQuery getUserGroupQuery = new() { TelegramId = request.UserId };
        GetUserGroupQueryHandler getUserGroupQueryHandler = new GetUserGroupQueryHandler(_unitOfWork, _userDto);

        await getUserGroupQueryHandler.Handle(getUserGroupQuery,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        return await Task.FromResult(new TrySetGroupReply { GroupName = _userDto.GroupName });
    }

    public override async Task<TryEnqueueInClassReply> TryEnqueueInClass(TryEnqueueInClassRequest request,
        ServerCallContext context)
    {
        CreateQueueCommand createQueueCommand = new CreateQueueCommand { TelegramId = request.UserId, ClassId = request.ClassId };
        CreateQueueCommandHandler createQueueCommandHandler = new CreateQueueCommandHandler(_unitOfWork);

        Result response = await createQueueCommandHandler.Handle(createQueueCommand,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (response.IsFailed)
            return await Task.FromResult(new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = response.Errors.ToString() });

        GetQueueQuery getQueueQuery = new GetQueueQuery { TelegramId = request.UserId, ClassId = request.ClassId };
        GetQueueQueryHandler getQueueQueryHandler = new GetQueueQueryHandler(_unitOfWork, _queueDto);

        await getQueueQueryHandler.Handle(getQueueQuery, new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        TryEnqueueInClassReply reply = new();

        await reply.StudentsQueue.FromList(_queueDto.QueueList);

        return await Task.FromResult(reply);
    }
}