using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Queries.GetGroup;
using DatabaseApp.Application.Group.Queries.GetGroups;
using DatabaseApp.Application.Queue;
using DatabaseApp.Application.Queue.Commands.CreateQueue;
using DatabaseApp.Application.Queue.Queries.GetQueue;
using DatabaseApp.Application.Queue.Queries.IsUserInQueue;
using DatabaseApp.Application.Subscriber;
using DatabaseApp.Application.Subscriber.Command.CreateSubscriber;
using DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;
using DatabaseApp.Application.Subscriber.Queries.GetSubscribers;
using DatabaseApp.Application.User;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Application.User.Queries.GetUserInfo;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using FluentResults;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseService(ISender mediator, ICacheService cacheService) : Database.DatabaseBase
{
    public override async Task<GetUserInfoReply> GetUserInfo(GetUserInfoRequest request, ServerCallContext context)
    {
        UserDto? user = await cacheService.GetAsync<UserDto>(Constants.UserPrefix + request.UserId);

        if (user is not null)
        {
            return new GetUserInfoReply { FullName = user.FullName, GroupName = user.GroupName };
        }

        Result<UserDto> userDto = await mediator.Send(new GetUserInfoQuery
        {
            TelegramId = request.UserId
        });

        if (userDto.IsFailed)
            return new GetUserInfoReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        await cacheService.SetAsync(Constants.UserPrefix + request.UserId, userDto.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI

        return new GetUserInfoReply { FullName = userDto.Value.FullName, GroupName = userDto.Value.GroupName };
    }

    public override async Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        List<GroupDto>? groups = await cacheService.GetAsync<List<GroupDto>>(Constants.AvailableGroupsKey);

        GetAvailableGroupsReply reply = new();
        
        if (groups is not null)
        { 
            foreach (var item in groups)
            {
                reply.IdGroupsMap.Add(item.Id, item.GroupName);
            }

            return reply;
        }
        
        Result<List<GroupDto>> groupDto = await mediator.Send(new GetGroupsQuery());

        if (groupDto.IsFailed) return new GetAvailableGroupsReply();
        
        await cacheService.SetAsync(Constants.AvailableGroupsKey, groupDto.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI

        foreach (var item in groupDto.Value)
        {
            reply.IdGroupsMap.Add(item.Id, item.GroupName);
        }

        return reply;
    }

    public override async Task<GetAvailableLabClassesReply> GetAvailableLabClasses(
        GetAvailableLabClassesRequest request, ServerCallContext context)
    {
        UserDto? userCache = await cacheService.GetAsync<UserDto>(Constants.UserPrefix + request.UserId);

        if (userCache is null)
        {
            Result<UserDto> userDto = await mediator.Send(new GetUserInfoQuery
            {
                TelegramId = request.UserId
            });

            if (userDto.IsFailed)
                return new GetAvailableLabClassesReply
                    { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

            await cacheService.SetAsync(Constants.UserPrefix + request.UserId, userDto.Value, 
                cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI

            userCache = userDto.Value;
        }

        List<ClassDto>? classes = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + userCache.GroupId);

        RepeatedField<ClassInformation> classInformation;
        
        if (classes is not null)
        {
            classInformation = await classes.ToRepeatedField<ClassInformation, ClassDto>(dto => new ClassInformation
            {
                ClassId = dto.Id,
                ClassName = dto.Name,
                ClassDateUnixTimestamp = ((DateTimeOffset)dto.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds()
            });
        
            return new GetAvailableLabClassesReply { ClassInformation = { classInformation } };
        }
        
        Result<List<ClassDto>> classDto = await mediator.Send(new GetClassesQuery
        {
            GroupName = userCache.GroupName
        });

        if (classDto.IsFailed)
            return new GetAvailableLabClassesReply
                { IsFailed = true, ErrorMessage = classDto.Errors.First().Message };

        classInformation = await classDto.Value.ToRepeatedField<ClassInformation, ClassDto>(dto => new ClassInformation
        {
            ClassId = dto.Id,
            ClassName = dto.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)dto.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds()
        });
        
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + userCache.GroupId, classDto.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI

        return new GetAvailableLabClassesReply { ClassInformation = { classInformation }};
    }

    public override async Task<TrySetGroupReply> TrySetGroup(TrySetGroupRequest request, ServerCallContext context)
    {
        Result result = await mediator.Send(new CreateUserCommand
        {
            TelegramId = request.UserId,
            FullName = request.FullName,
            GroupName = request.GroupName
        });

        if (result.IsFailed)
            return new TrySetGroupReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        Result<UserDto> userDto = await mediator.Send(new GetUserInfoQuery
        {
            TelegramId = request.UserId
        });

        if (userDto.IsFailed)
            return new TrySetGroupReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        await cacheService.SetAsync(Constants.UserPrefix + request.UserId, userDto.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI

        return new TrySetGroupReply { FullName = await request.FullName.FormatFio(), GroupName = userDto.Value.GroupName };
    }

    public override async Task<TryEnqueueInClassReply> TryEnqueueInClass(TryEnqueueInClassRequest request,
        ServerCallContext context)
    {
        List<QueueDto>? queueCache = await cacheService.GetAsync<List<QueueDto>>(Constants.QueuePrefix + request.ClassId);
        
        if (queueCache is null)
        {
            Result<List<QueueDto>> queueResult = await mediator.Send(new GetClassQueueQuery
            {
                ClassId = request.ClassId
            });

            if (queueResult.IsFailed)
                return new TryEnqueueInClassReply 
                    { IsFailed = true, ErrorMessage = queueResult.Errors.First().Message };
            
            queueCache = queueResult.Value;  
        }
        
        UserDto? user = await cacheService.GetAsync<UserDto>(Constants.UserPrefix + request.UserId);

        if (user is null)
        {
            Result<UserDto> userDto = await mediator.Send(new GetUserInfoQuery
            {
                TelegramId = request.UserId
            });

            if (userDto.IsFailed)
                return new TryEnqueueInClassReply
                    { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

            await cacheService.SetAsync(Constants.UserPrefix + request.UserId, userDto.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

            user = userDto.Value;
        }

        List<ClassDto>? classes = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + user.GroupId);

        if (classes is null)
        {
            Result<List<ClassDto>> classDto = await mediator.Send(new GetClassesQuery
            {
                GroupName = user.GroupName
            });

            if (classDto.IsFailed || classDto.Value.Count == 0)
                return new TryEnqueueInClassReply
                    { IsFailed = true, ErrorMessage = classDto.Errors.First().Message };
        
            await cacheService.SetAsync(Constants.AvailableClassesPrefix + user.GroupId, classDto.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

            classes = classDto.Value;
        }

        ClassDto @class;
        try
        {
            @class = classes.First(x => x.Id == request.ClassId);
        }
        catch (Exception e)
        {
            return new TryEnqueueInClassReply { IsFailed = true, ErrorMessage = e.Message };
        }

        Result<UserDto?> userInQueueResult = await mediator.Send(new GetUserInQueueQuery
        {
            ClassId = request.ClassId,
            TelegramId = request.UserId
        });

        if (userInQueueResult.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = userInQueueResult.Errors.First().Message };
        
        if (userInQueueResult.Value is not null)
        {
            return new TryEnqueueInClassReply
            {
                WasAlreadyEnqueued = true, 
                ClassName = @class.Name, 
                ClassDateUnixTimestamp = ((DateTimeOffset)@class.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
                StudentsQueue = { queueCache.Select(x => x.FullName) }
            };
        }
        
        // If we have to ACTUALLY enqueue user
        Result result = await mediator.Send(new CreateQueueCommand
        {
            TelegramId = request.UserId,
            ClassId = request.ClassId
        });
        
        if (result.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };
        
        queueCache.Add(new QueueDto
        {
            ClassId = request.ClassId,
            FullName = user.FullName
        });

        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, queueCache, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI

        return new TryEnqueueInClassReply
        {
            ClassName = @class.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)@class.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            StudentsQueue = { queueCache.Select(x => x.FullName) }
        };
    }

    public override async Task<AddSubscriberReply> AddSubscriber(AddSubscriberRequest request, ServerCallContext context)
    {
        Result result = await mediator.Send(new CreateSubscriberCommand
        {
            TelegramId = request.SubscriberId
        });

        if (result.IsFailed)
            return new AddSubscriberReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };
        
        Result<UserDto> userDto = await mediator.Send(new GetUserInfoQuery
        {
            TelegramId = request.SubscriberId
        });
        
        if (result.IsFailed)
            return new AddSubscriberReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };
        
        Result<GroupDto> groupDto = await mediator.Send(new GetGroupQuery
        {
            GroupName = userDto.Value.GroupName
        });
        
        if (result.IsFailed)
            return new AddSubscriberReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };
        
        List<SubscriberDto> cachedSubscriptions = await cacheService.GetAsync<List<SubscriberDto>>(Constants.AllSubscribersKey) ?? [];
        cachedSubscriptions.Add(new SubscriberDto { TelegramId = request.SubscriberId, GroupId = groupDto.Value.Id});
        
        await cacheService.SetAsync(Constants.AllSubscribersKey, cachedSubscriptions, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI
        
        return new AddSubscriberReply();
    }

    public override async Task<DeleteSubscriberReply> DeleteSubscriber(DeleteSubscriberRequest request, ServerCallContext context)
    {
        Result result = await mediator.Send(new DeleteSubscriberCommand
        {
            TelegramId = request.SubscriberId
        });

        if (result.IsFailed)
            return new DeleteSubscriberReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        List<SubscriberDto> cachedSubscriptions = await cacheService.GetAsync<List<SubscriberDto>>(Constants.AllSubscribersKey) ?? [];
        
        await cacheService.SetAsync(Constants.AllSubscribersKey, cachedSubscriptions.Where(x => x.TelegramId != request.SubscriberId),
            cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI
        
        return new DeleteSubscriberReply();
    }

    public override async Task<GetSubscribersReply> GetSubscribers(Empty request, ServerCallContext context)
    {
        List<SubscriberDto>? subscriberDto = await cacheService.GetAsync<List<SubscriberDto>>(Constants.AllSubscribersKey);

        if (subscriberDto is null)
        {
            Result<List<SubscriberDto>> subscribersResult = await mediator.Send(new GetAllSubscribersQuery());
            
            if (subscribersResult.IsFailed)
                return new GetSubscribersReply
                    { IsFailed = true, ErrorMessage = subscribersResult.Errors.First().Message };

            subscriberDto = subscribersResult.Value;
            
            await cacheService.SetAsync(Constants.AllSubscribersKey, subscriberDto, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI
        }

        RepeatedField<SubscriberInformation> repeatedField = await subscriberDto.ToRepeatedField<SubscriberInformation, SubscriberDto>(dto => new SubscriberInformation
        {
            UserId = dto.TelegramId,
            GroupId = dto.GroupId
        });
        
        return new GetSubscribersReply { Subscribers = { repeatedField } };
    }
}