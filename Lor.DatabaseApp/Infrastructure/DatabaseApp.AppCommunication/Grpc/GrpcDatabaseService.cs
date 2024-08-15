using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Queries.GetClass;
using DatabaseApp.Application.Class.Queries.GetClasses;
using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Group.Queries.GetGroups;
using DatabaseApp.Application.Queue;
using DatabaseApp.Application.Queue.Commands.CreateQueue;
using DatabaseApp.Application.Queue.Queries.GetQueue;
using DatabaseApp.Application.User;
using DatabaseApp.Application.User.Command.CreateUser;
using DatabaseApp.Application.User.Queries.GetUserInfo;
using FluentResults;
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
        });

        if (userDto.IsFailed)
            return new GetUserInfoReply
                { IsFailed = true, ErrorMessage = userDto.Errors.First().Message };

        return new GetUserInfoReply { FullName = userDto.Value.FullName, GroupName = userDto.Value.GroupName };
    }

    public override async Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        Result<List<GroupDto>> groupDto = await mediator.Send(new GetGroupsQuery());

        if (groupDto.IsFailed) return new GetAvailableGroupsReply();

        GetAvailableGroupsReply reply = new();

        foreach (var item in groupDto.Value)
        {
            reply.IdGroupsMap.Add(item.Id, item.GroupName);
        }

        return reply;
    }

    public override async Task<GetAvailableLabClassesReply> GetAvailableLabClasses(
        GetAvailableLabClassesRequest request, ServerCallContext context)
    {
        Result<List<ClassDto>> classDto = await mediator.Send(new GetClassesQuery
        {
            TelegramId = request.UserId
        });

        if (classDto.IsFailed)
            return new GetAvailableLabClassesReply
                { IsFailed = true, ErrorMessage = classDto.Errors.First().Message };

        GetAvailableLabClassesReply reply = new();

        await reply.ClassInformation.FromList<ClassInformation, ClassDto>(classDto.Value, dto => new ClassInformation()
        {
            ClassId = dto.Id,
            ClassName = dto.Name,
            ClassDateUnixTimestamp = ((DateTimeOffset)dto.Date.ToDateTime(TimeOnly.MinValue)).ToUnixTimeSeconds()
        });

        return reply;
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
        
        TryEnqueueInClassReply reply = new();
        
        foreach (var item in queueDto.Value)
        {
            reply.StudentsQueue.Add(item.FullName);
        }
        
        reply.ClassName = classDto.Value.Name;
        reply.ClassDateUnixTimestamp = ((DateTimeOffset)classDto.Value.Date.ToDateTime(TimeOnly.MinValue)).ToUnixTimeSeconds();
        
        return reply;
    }
}