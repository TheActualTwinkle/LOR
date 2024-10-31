using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Queries.GetClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Application.Group.Queries.GetGroups;
using DatabaseApp.Application.Queue.Commands.CreateQueue;
using DatabaseApp.Application.Queue.Commands.DeleteQueue;
using DatabaseApp.Application.Queue.Queries.GetQueue;
using DatabaseApp.Application.Queue.Queries.IsUserInQueue;
using DatabaseApp.Application.Subscriber;
using DatabaseApp.Application.Subscriber.Command.CreateSubscriber;
using DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;
using DatabaseApp.Application.Subscriber.Queries.GetSubscribers;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Application.User.Queries.GetUserInfo;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseService(ISender mediator) : Database.DatabaseBase
{
    public override async Task<GetUserInfoReply> GetUserInfo(GetUserInfoRequest request, ServerCallContext context)
    {
        var userDto = await mediator.Send(new GetUserInfoQuery
        {
            TelegramId = request.UserId
        }, context.CancellationToken);

        if (userDto.IsFailed)
            return new GetUserInfoReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        return new GetUserInfoReply { FullName = userDto.Value.FullName, GroupName = userDto.Value.GroupName };
    }

    public override async Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        GetAvailableGroupsReply reply = new();
        
        var groups = await mediator.Send(new GetGroupsQuery(), context.CancellationToken);

        if (groups.IsFailed) return new GetAvailableGroupsReply();
        
        foreach (var item in groups.Value)
            reply.IdGroupsMap.Add(item.Id, item.GroupName);

        return reply;
    }

    public override async Task<GetAvailableClassesReply> GetAvailableClasses(
        GetAvailableClassesRequest request, ServerCallContext context)
    {
        var userDto = await mediator.Send(new GetUserInfoQuery
        {
            TelegramId = request.UserId
        }, context.CancellationToken);

        if (userDto.IsFailed)
            return new GetAvailableClassesReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        var classes = await mediator.Send(new GetClassesQuery
        {
            GroupId = userDto.Value.GroupId
        }, context.CancellationToken);

        if (classes.IsFailed)
            return new GetAvailableClassesReply
                { IsFailed = true, ErrorMessage = classes.Errors.First().Message };

        var classInformation = await classes.Value.ToRepeatedField<ClassInformation, ClassDto>(dto => new ClassInformation
        {
            ClassId = dto.Id,
            ClassName = dto.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)dto.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds()
        });
        
        return new GetAvailableClassesReply { ClassInformation = { classInformation }};
    }

    public override async Task<SetGroupReply> SetGroup(SetGroupRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(new CreateUserCommand
        {
            TelegramId = request.UserId,
            FullName = request.FullName,
            GroupName = request.GroupName
        }, context.CancellationToken);

        if (result.IsFailed)
            return new SetGroupReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        var userDto = await mediator.Send(new GetUserInfoQuery
        {
            TelegramId = request.UserId
        }, context.CancellationToken);

        if (userDto.IsFailed)
            return new SetGroupReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        return new SetGroupReply { FullName = await request.FullName.FormatFio(), GroupName = userDto.Value.GroupName };
    }

    public override async Task<EnqueueInClassReply> EnqueueInClass(EnqueueInClassRequest request,
        ServerCallContext context)
    {
        var getClassResult = await mediator.Send(new GetClassQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);
        
        var userInQueue = await mediator.Send(new GetUserInQueueQuery
        {
            ClassId = request.ClassId,
            TelegramId = request.UserId
        });
        
        if (userInQueue.IsFailed)
            return new EnqueueInClassReply
                { IsFailed = true, ErrorMessage = userInQueue.Errors.First().Message };
        
        var queueDto = await mediator.Send(new GetClassQueueQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);
        
        if (queueDto.IsFailed)
            return new EnqueueInClassReply
                { IsFailed = true, ErrorMessage = queueDto.Errors.First().Message };

        // If user already in queue
        if (userInQueue.Value is not null)
            return new EnqueueInClassReply
            {
                WasAlreadyEnqueued = true,
                ClassName = getClassResult.Value.Name,
                ClassDateUnixTimestamp = ((DateTimeOffset)getClassResult.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
                StudentsQueue = { queueDto.Value.Select(x => x.FullName) }
            };

        // If we have to ACTUALLY enqueue user
        var result = await mediator.Send(new CreateQueueCommand
        {
            TelegramId = request.UserId,
            ClassId = request.ClassId
        }, context.CancellationToken);
        
        if (result.IsFailed)
            return new EnqueueInClassReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };
        
        queueDto = await mediator.Send(new GetClassQueueQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);
        
        if (queueDto.IsFailed)
            return new EnqueueInClassReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        return new EnqueueInClassReply
        {
            ClassName = getClassResult.Value.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)getClassResult.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            StudentsQueue = { queueDto.Value.Select(x => x.FullName) }
        };
    }
    
    public override async Task<DequeueFromClassReply> DequeueFromClass(DequeueFromClassRequest request, ServerCallContext context)
    {
        var getClassResult = await mediator.Send(new GetClassQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);

        if (getClassResult.IsFailed)
            return new DequeueFromClassReply
                { IsFailed = true, ErrorMessage = getClassResult.Errors.First().Message };
        
        var queueDto = await mediator.Send(new GetClassQueueQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);

        if (queueDto.IsFailed)
            return new DequeueFromClassReply 
                { IsFailed = true, ErrorMessage = queueDto.Errors.First().Message };
        
        var userInQueue = await mediator.Send(new GetUserInQueueQuery
        {
            ClassId = request.ClassId,
            TelegramId = request.UserId
        });
        
        if (userInQueue.IsFailed)
            return new DequeueFromClassReply
                { IsFailed = true, ErrorMessage = userInQueue.Errors.First().Message };

        // If user is not in queue
        if (userInQueue.Value is null)
            return new DequeueFromClassReply
            {
                WasAlreadyDequeuedFromClass = true,
                ClassName = getClassResult.Value.Name,
                ClassDateUnixTimestamp = ((DateTimeOffset)getClassResult.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
                StudentsQueue = { queueDto.Value.Select(x => x.FullName) }
            };

        // If we have to ACTUALLY DequeueFromClass user
        var result = await mediator.Send(new DeleteUserFromQueueCommand
        {
            ClassId = request.ClassId,
            TelegramId = request.UserId
        }, context.CancellationToken);
            
        if (result.IsFailed) return new DequeueFromClassReply
            { IsFailed = true, ErrorMessage = result.Errors.First().Message };
            
        queueDto = await mediator.Send(new GetClassQueueQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);

        if (queueDto.IsFailed)
            return new DequeueFromClassReply 
                { IsFailed = true, ErrorMessage = queueDto.Errors.First().Message };

        return new DequeueFromClassReply
        {
            ClassName = getClassResult.Value.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)getClassResult.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            StudentsQueue = { queueDto.Value.Select(x => x.FullName) }
        };
    }

    public override async Task<AddSubscriberReply> AddSubscriber(AddSubscriberRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(new CreateSubscriberCommand
        {
            TelegramId = request.SubscriberId
        }, context.CancellationToken);

        if (result.IsFailed)
            return new AddSubscriberReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };
        
        return new AddSubscriberReply();
    }
    
    public override async Task<DeleteSubscriberReply> DeleteSubscriber(DeleteSubscriberRequest request, ServerCallContext context)
    {
        var result = await mediator.Send(new DeleteSubscriberCommand
        {
            TelegramId = request.SubscriberId
        }, context.CancellationToken);

        if (result.IsFailed)
            return new DeleteSubscriberReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };
        
        return new DeleteSubscriberReply();
    }

    public override async Task<GetSubscribersReply> GetSubscribers(Empty request, ServerCallContext context)
    {
        var subscriberDto = await mediator.Send(new GetAllSubscribersQuery(), context.CancellationToken);
            
        if (subscriberDto.IsFailed)
            return new GetSubscribersReply
                { IsFailed = true, ErrorMessage = subscriberDto.Errors.First().Message };

        var repeatedField = await subscriberDto.Value.ToRepeatedField<SubscriberInformation, SubscriberDto>(dto => new SubscriberInformation
        {
            UserId = dto.TelegramId,
            GroupId = dto.GroupId
        });
        
        return new GetSubscribersReply { Subscribers = { repeatedField } };
    }
}