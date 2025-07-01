using DatabaseApp.Application.Classes;
using DatabaseApp.Application.Classes.Queries;
using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Application.Group.Queries;
using DatabaseApp.Application.QueueEntries.Commands.CreateEntry;
using DatabaseApp.Application.QueueEntries.Commands.DeleteEntry;
using DatabaseApp.Application.QueueEntries.Queries;
using DatabaseApp.Application.Subscriber;
using DatabaseApp.Application.Subscriber.Command.CreateSubscriber;
using DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;
using DatabaseApp.Application.Subscriber.Queries;
using DatabaseApp.Application.Users.Command.CreateUser;
using DatabaseApp.Application.Users.Queries;
using DatabaseApp.Domain.Repositories;
using DatabaseApp.WebApi.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace DatabaseApp.WebApi.GrpcServices;

public class GrpcDatabaseService(
    ISender mediator,
    IUnitOfWork unitOfWork)
    : Database.DatabaseBase
{
    public override async Task<GetUserInfoReply> GetUserInfo(GetUserInfoRequest request, ServerCallContext context)
    {
        var userDto = await mediator.Send(
            new GetUserInfoQuery
            {
                TelegramId = request.TelegramId
            }, context.CancellationToken);

        if (userDto.IsFailed)
            return new GetUserInfoReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        return new GetUserInfoReply
        {
            FullName = userDto.Value.FullName,
            GroupName = userDto.Value.GroupName
        };
    }

    public override async Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        GetAvailableGroupsReply reply = new();

        var groups = await mediator.Send(new GetGroupsQuery(), context.CancellationToken);

        if (groups.IsFailed)
            return new GetAvailableGroupsReply();

        foreach (var item in groups.Value)
            reply.IdGroupsMap.Add(item.Id, item.GroupName);

        return reply;
    }

    public override async Task<GetAvailableClassesReply> GetAvailableClasses(
        GetAvailableClassesRequest request,
        ServerCallContext context)
    {
        var userDto = await mediator.Send(
            new GetUserInfoQuery
            {
                TelegramId = request.TelegramId
            }, context.CancellationToken);

        if (userDto.IsFailed)
            return new GetAvailableClassesReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        var classes = await mediator.Send(
            new GetClassesQuery
            {
                GroupName = userDto.Value.GroupName
            }, context.CancellationToken);

        if (classes.IsFailed)
            return new GetAvailableClassesReply
                { IsFailed = true, ErrorMessage = classes.Errors.First().Message };

        var classInformation = await classes.Value.ToRepeatedField<ClassInformation, ClassDto>(dto => new ClassInformation
        {
            Id = dto.Id,
            Name = dto.Name,
            ClassDateUnixTimestamp = dto.Date.ToUnixTime()
        });

        return new GetAvailableClassesReply { ClassInformation = { classInformation } };
    }

    public override async Task<SetGroupReply> SetGroup(SetGroupRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new CreateUserCommand
            {
                TelegramId = request.TelegramId,
                FullName = request.FullName,
                GroupName = request.GroupName
            }, context.CancellationToken);

        if (result.IsFailed)
            return new SetGroupReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        var userDto = await mediator.Send(
            new GetUserInfoQuery
            {
                TelegramId = request.TelegramId
            }, context.CancellationToken);

        if (userDto.IsFailed)
            return new SetGroupReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        return new SetGroupReply { FullName = FullNameFormatter.Format(request.FullName), GroupName = userDto.Value.GroupName };
    }

    public override async Task<EnqueueInClassReply> EnqueueInClass(
        EnqueueInClassRequest request,
        ServerCallContext context)
    {
        var createQueueEntryResult = await mediator.Send(
            new CreateQueueEntryCommand
            {
                TelegramId = request.TelegramId,
                ClassId = request.ClassId
            },
            context.CancellationToken);

        if (createQueueEntryResult.IsFailed)
            return new EnqueueInClassReply
                { IsFailed = true, ErrorMessage = createQueueEntryResult.Errors.First().Message };

        var queue = await mediator.Send(
            new GetClassQueueQuery
            {
                ClassId = request.ClassId
            },
            context.CancellationToken);

        if (queue.IsFailed)
            return new EnqueueInClassReply
                { IsFailed = true, ErrorMessage = createQueueEntryResult.Errors.First().Message };

        var createQueueEntryResponse = createQueueEntryResult.Value;

        return new EnqueueInClassReply
        {
            Class = new ClassInformation
            {
                Id = createQueueEntryResponse.Class.Id,
                Name = createQueueEntryResponse.Class.Name,
                ClassDateUnixTimestamp = createQueueEntryResponse.Class.Date.ToUnixTime()
            },
            StudentsQueue = { queue.Value.Select(x => x.FullName) },
            WasAlreadyEnqueued = createQueueEntryResponse.WasAlreadyEnqueued
        };
    }

    public override async Task<DequeueFromClassReply> DequeueFromClass(DequeueFromClassRequest request, ServerCallContext context)
    {
        
        var deleteQueueEntryResult = await mediator.Send(
            new DeleteQueueEntryCommand
            {
                ClassId = request.ClassId,
                TelegramId = request.TelegramId
            }, 
            context.CancellationToken);

        if (deleteQueueEntryResult.IsFailed)
            return new DequeueFromClassReply
                { IsFailed = true, ErrorMessage = deleteQueueEntryResult.Errors.First().Message };

        var queue = await mediator.Send(
            new GetClassQueueQuery
            {
                ClassId = request.ClassId
            }, context.CancellationToken);

        if (queue.IsFailed)
            return new DequeueFromClassReply
                { IsFailed = true, ErrorMessage = queue.Errors.First().Message };

        var deleteQueueEntryResponse = deleteQueueEntryResult.Value;
        
        return new DequeueFromClassReply
        {
            Class = new ClassInformation
            {
                Id = deleteQueueEntryResponse.Class.Id,
                Name = deleteQueueEntryResponse.Class.Name,
                ClassDateUnixTimestamp = deleteQueueEntryResponse.Class.Date.ToUnixTime()
            },
            StudentsQueue = { queue.Value.Select(x => x.FullName) },
            WasAlreadyDequeuedFromClass = deleteQueueEntryResponse.WasAlreadyDequeued
        };
    }

    public override async Task<ViewQueueClassReply> ViewQueueClass(ViewQueueClassRequest request, ServerCallContext context)
    {
        var queue = await mediator.Send(
            new GetClassQueueQuery
            {
                ClassId = request.ClassId
            }, context.CancellationToken);

        if (queue.IsFailed)
            return new ViewQueueClassReply
                { IsFailed = true, ErrorMessage = queue.Errors.First().Message };

        var @class = await unitOfWork.ClassRepository
            .GetClassById(request.ClassId, context.CancellationToken);

        if (@class is null)
            return new ViewQueueClassReply
                { IsFailed = true, ErrorMessage = "Данной пары не существует" };

        return new ViewQueueClassReply
        {
            Class = new ClassInformation
            {
                Id = request.ClassId,
                Name = @class.Name,
                ClassDateUnixTimestamp = @class.Date.ToUnixTime()
            },
            StudentsQueue = { queue.Value.Select(x => x.FullName) }
        };
    }

    public override async Task<GetSubscribersReply> GetSubscribers(Empty request, ServerCallContext context)
    {
        var subscriberDto = await mediator.Send(new GetAllSubscribersQuery(), context.CancellationToken);

        if (subscriberDto.IsFailed)
            return new GetSubscribersReply
                { IsFailed = true, ErrorMessage = subscriberDto.Errors.First().Message };

        var repeatedField = await subscriberDto.Value.ToRepeatedField<SubscriberInformation, SubscriberDto>(dto => new SubscriberInformation
        {
            TelegramId = dto.TelegramId,
            GroupName = dto.GroupName
        });

        return new GetSubscribersReply { Subscribers = { repeatedField } };
    }

    public override async Task<AddSubscriberReply> AddSubscriber(AddSubscriberRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new CreateSubscriberCommand
            {
                TelegramId = request.TelegramId
            }, context.CancellationToken);

        if (result.IsFailed)
            return new AddSubscriberReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        return new AddSubscriberReply();
    }

    public override async Task<DeleteSubscriberReply> DeleteSubscriber(DeleteSubscriberRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(
            new DeleteSubscriberCommand
            {
                TelegramId = request.TelegramId
            }, context.CancellationToken);

        if (result.IsFailed)
            return new DeleteSubscriberReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        return new DeleteSubscriberReply();
    }
}