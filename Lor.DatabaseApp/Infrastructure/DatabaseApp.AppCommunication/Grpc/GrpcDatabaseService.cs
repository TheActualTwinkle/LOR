﻿using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Queries.GetClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Queries.GetGroups;
using DatabaseApp.Application.Queue;
using DatabaseApp.Application.Queue.Commands.CreateQueue;
using DatabaseApp.Application.Queue.Commands.DeleteQueue;
using DatabaseApp.Application.Queue.Queries.GetQueue;
using DatabaseApp.Application.Queue.Queries.IsUserInQueue;
using DatabaseApp.Application.Subscriber;
using DatabaseApp.Application.Subscriber.Command.CreateSubscriber;
using DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;
using DatabaseApp.Application.Subscriber.Queries.GetSubscribers;
using DatabaseApp.Application.User;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Application.User.Queries.GetUserInfo;
using FluentResults;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseService(ISender mediator) : Database.DatabaseBase
{
    public override async Task<GetUserInfoReply> GetUserInfo(GetUserInfoRequest request, ServerCallContext context)
    {
        Result<UserDto> userDto = await mediator.Send(new GetUserInfoQuery
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
        
        Result<List<GroupDto>> groups = await mediator.Send(new GetGroupsQuery(), context.CancellationToken);

        if (groups.IsFailed) return new GetAvailableGroupsReply();
        
        foreach (GroupDto item in groups.Value)
        {
            reply.IdGroupsMap.Add(item.Id, item.GroupName);
        }

        return reply;
    }

    public override async Task<GetAvailableLabClassesReply> GetAvailableLabClasses(
        GetAvailableLabClassesRequest request, ServerCallContext context)
    {
        Result<UserDto> userDto = await mediator.Send(new GetUserInfoQuery
        {
            TelegramId = request.UserId
        }, context.CancellationToken);

        if (userDto.IsFailed)
            return new GetAvailableLabClassesReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        Result<List<ClassDto>> classes = await mediator.Send(new GetClassesQuery
        {
            GroupId = userDto.Value.GroupId
        }, context.CancellationToken);

        if (classes.IsFailed)
            return new GetAvailableLabClassesReply
                { IsFailed = true, ErrorMessage = classes.Errors.First().Message };

        RepeatedField<ClassInformation> classInformation = await classes.Value.ToRepeatedField<ClassInformation, ClassDto>(dto => new ClassInformation
        {
            ClassId = dto.Id,
            ClassName = dto.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)dto.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds()
        });
        
        return new GetAvailableLabClassesReply { ClassInformation = { classInformation }};
    }

    public override async Task<TrySetGroupReply> TrySetGroup(TrySetGroupRequest request, ServerCallContext context)
    {
        Result result = await mediator.Send(new CreateUserCommand
        {
            TelegramId = request.UserId,
            FullName = request.FullName,
            GroupName = request.GroupName
        }, context.CancellationToken);

        if (result.IsFailed)
            return new TrySetGroupReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        Result<UserDto> userDto = await mediator.Send(new GetUserInfoQuery
        {
            TelegramId = request.UserId
        }, context.CancellationToken);

        if (userDto.IsFailed)
            return new TrySetGroupReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        return new TrySetGroupReply { FullName = await request.FullName.FormatFio(), GroupName = userDto.Value.GroupName };
    }

    public override async Task<TryEnqueueInClassReply> TryEnqueueInClass(TryEnqueueInClassRequest request,
        ServerCallContext context)
    {
        Result<ClassDto> getClassResult = await mediator.Send(new GetClassQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);
        
        Result<UserDto?> userInQueue = await mediator.Send(new GetUserInQueueQuery
        {
            ClassId = request.ClassId,
            TelegramId = request.UserId
        });
        
        if (userInQueue.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = userInQueue.Errors.First().Message };
        
        Result<List<QueueDto>> queueDto = await mediator.Send(new GetClassQueueQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);
        
        if (queueDto.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = queueDto.Errors.First().Message };

        // If user already in queue
        if (userInQueue.Value is not null)
        {
            return new TryEnqueueInClassReply
            {
                WasAlreadyEnqueued = true,
                ClassName = getClassResult.Value.Name,
                ClassDateUnixTimestamp = ((DateTimeOffset)getClassResult.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
                StudentsQueue = { queueDto.Value.Select(x => x.FullName) }
            };
        }
        
        // If we have to ACTUALLY enqueue user
        Result result = await mediator.Send(new CreateQueueCommand
        {
            TelegramId = request.UserId,
            ClassId = request.ClassId
        }, context.CancellationToken);
        
        if (result.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };
        
        queueDto = await mediator.Send(new GetClassQueueQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);
        
        if (queueDto.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        return new TryEnqueueInClassReply
        {
            ClassName = getClassResult.Value.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)getClassResult.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            StudentsQueue = { queueDto.Value.Select(x => x.FullName) }
        };
    }
    
    public override async Task<DequeueReply> Dequeue(DequeueRequest request, ServerCallContext context)
    {
        Result<ClassDto> getClassResult = await mediator.Send(new GetClassQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);

        if (getClassResult.IsFailed)
            return new DequeueReply
                { IsFailed = true, ErrorMessage = getClassResult.Errors.First().Message };
        
        Result<List<QueueDto>> queueDto = await mediator.Send(new GetClassQueueQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);

        if (queueDto.IsFailed)
            return new DequeueReply 
                { IsFailed = true, ErrorMessage = queueDto.Errors.First().Message };
        
        Result<UserDto?> userInQueue = await mediator.Send(new GetUserInQueueQuery
        {
            ClassId = request.ClassId,
            TelegramId = request.UserId
        });
        
        if (userInQueue.IsFailed)
            return new DequeueReply
                { IsFailed = true, ErrorMessage = userInQueue.Errors.First().Message };

        // If user is not in queue
        if (userInQueue.Value is null)
        {
            return new DequeueReply
            {
                WasAlreadyDequeued = true,
                ClassName = getClassResult.Value.Name,
                ClassDateUnixTimestamp = ((DateTimeOffset)getClassResult.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
                StudentsQueue = { queueDto.Value.Select(x => x.FullName) }
            };
        }

        // If we have to ACTUALLY dequeue user
        Result result = await mediator.Send(new DeleteUserFromQueueCommand
        {
            ClassId = request.ClassId,
            TelegramId = request.UserId
        }, context.CancellationToken);
            
        if (result.IsFailed) return new DequeueReply
            { IsFailed = true, ErrorMessage = result.Errors.First().Message };
            
        queueDto = await mediator.Send(new GetClassQueueQuery
        {
            ClassId = request.ClassId
        }, context.CancellationToken);

        if (queueDto.IsFailed)
            return new DequeueReply 
                { IsFailed = true, ErrorMessage = queueDto.Errors.First().Message };

        return new DequeueReply
        {
            ClassName = getClassResult.Value.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)getClassResult.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            StudentsQueue = { queueDto.Value.Select(x => x.FullName) }
        };
    }

    public override async Task<AddSubscriberReply> AddSubscriber(AddSubscriberRequest request, ServerCallContext context)
    {
        Result result = await mediator.Send(new CreateSubscriberCommand
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
        Result result = await mediator.Send(new DeleteSubscriberCommand
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
        Result<List<SubscriberDto>> subscriberDto = await mediator.Send(new GetAllSubscribersQuery(), context.CancellationToken);
            
        if (subscriberDto.IsFailed)
            return new GetSubscribersReply
                { IsFailed = true, ErrorMessage = subscriberDto.Errors.First().Message };

        RepeatedField<SubscriberInformation> repeatedField = await subscriberDto.Value.ToRepeatedField<SubscriberInformation, SubscriberDto>(dto => new SubscriberInformation
        {
            UserId = dto.TelegramId,
            GroupId = dto.GroupId
        });
        
        return new GetSubscribersReply { Subscribers = { repeatedField } };
    }
}