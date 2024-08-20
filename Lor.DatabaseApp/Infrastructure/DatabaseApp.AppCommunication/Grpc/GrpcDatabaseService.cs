using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Queries.GetClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Queries.GetGroupInfo;
using DatabaseApp.Application.Group.Queries.GetGroups;
using DatabaseApp.Application.Queue;
using DatabaseApp.Application.Queue.Commands.CreateQueue;
using DatabaseApp.Application.Queue.Queries.GetQueue;
using DatabaseApp.Application.User;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Application.User.Queries.GetUserInfo;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Models;
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
        
        if (groups is not null )
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
        Result<GroupDto> group = await mediator.Send(new GetGroupInfoQuery
        {
            TelegramId = request.UserId
        });
        
        List<ClassDto>? classes = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + group.Value.Id);

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
            TelegramId = request.UserId
        });

        if (classDto.IsFailed || classDto.Value.Count == 0)
            return new GetAvailableLabClassesReply
                { IsFailed = true, ErrorMessage = classDto.Errors.First().Message };

        classInformation = await classDto.Value.ToRepeatedField<ClassInformation, ClassDto>(dto => new ClassInformation
        {
            ClassId = dto.Id,
            ClassName = dto.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)dto.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds()
        });
        
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + group.Value.Id, classDto.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI

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
        Result<ClassDto> classDto = await mediator.Send(new GetClassQuery
        {
            ClassId = request.ClassId
        });
        
        if (classDto.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = classDto.Errors.First().Message };


        List<QueueDto>? queue = await cacheService.GetAsync<List<QueueDto>>(Constants.QueuePrefix + request.ClassId);
        
        TryEnqueueInClassReply reply = new();
        
        if (queue is not null)
        {
            foreach (var item in queue)
            {
                reply.StudentsQueue.Add(item.FullName);
            }
        
            reply.ClassName = classDto.Value.Name;
            reply.ClassDateUnixTimestamp = ((DateTimeOffset)classDto.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds();
        
            return reply;
        }
        
        Result result = await mediator.Send(new CreateQueueCommand
        {
            TelegramId = request.UserId,
            ClassId = request.ClassId
        });
        
        if (result.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = result.Errors.First().Message };

        Result<List<QueueDto>> queueDto = await mediator.Send(new GetQueueQuery
        {
            TelegramId = request.UserId,
            ClassId = request.ClassId
        });

        if (queueDto.IsFailed)
            return new TryEnqueueInClassReply
                { IsFailed = true, ErrorMessage = queueDto.Errors.First().Message };

        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, queueDto.Value, cancellationToken: new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI
        
        foreach (var item in queueDto.Value)
        {
            reply.StudentsQueue.Add(item.FullName);
        }
        
        reply.ClassName = classDto.Value.Name;
        reply.ClassDateUnixTimestamp = ((DateTimeOffset)classDto.Value.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds();
        
        return reply;
    }
}