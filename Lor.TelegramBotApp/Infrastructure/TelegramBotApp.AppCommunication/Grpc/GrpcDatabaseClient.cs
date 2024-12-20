using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using TelegramBotApp.AppCommunication.Consumers.Data;
using TelegramBotApp.AppCommunication.Data;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public class GrpcDatabaseClient(string serviceUrl, ILogger<GrpcDatabaseClient> logger) : IDatabaseCommunicationClient
{
    private Database.DatabaseClient? _client;
    
    public async Task StartAsync()
    {
        logger.LogInformation("Connecting to the Database gRPC service...");
        var channel = GrpcChannel.ForAddress(serviceUrl);
        await channel.ConnectAsync();
        logger.LogInformation("Successfully connected to the Database gRPC service.");
        
        _client = new Database.DatabaseClient(channel);
    }

    public Task StopAsync()
    {
        logger.LogInformation("Disconnecting from the Database gRPC service...");
        _client = null;
        logger.LogInformation("Successfully disconnected from the Database gRPC service.");
        
        return Task.CompletedTask;
    }

    public async Task<Result<UserInfo>> GetUserInfo(long userId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetUserInfoAsync(new GetUserInfoRequest { UserId = userId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail("Вы не авторизованы. Для авторизации введите /auth") : Result.Ok(new UserInfo { FullName = reply.FullName, GroupName = reply.GroupName });
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetAvailableGroupsAsync(new Empty(), cancellationToken: cancellationToken);
        var groups = reply.IdGroupsMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        
        return Result.Ok(groups);
    }

    public async Task<Result<IEnumerable<Class>>> GetAvailableLabClasses(long userId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetAvailableClassesAsync(new GetAvailableClassesRequest { UserId = userId }, cancellationToken: cancellationToken);
        
        if (reply.IsFailed) return Result.Fail(reply.ErrorMessage);

        return Result.Ok(reply.ClassInformation.Select(x => new Class
        {
            Id = x.ClassId,
            Name = x.ClassName,
            Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(x.ClassDateUnixTimestamp).DateTime)
        }));
    }

    public async Task<Result<string>> SetGroup(long userId, string groupName, string fullName, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.SetGroupAsync(new SetGroupRequest { UserId = userId, GroupName = groupName, FullName = fullName}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok($"{reply.FullName}: группа {reply.GroupName} успешно установлена!");
    }

    public async Task<Result<EnqueueInClassResult>> EnqueueInClass(int classId, long userId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.EnqueueInClassAsync(new EnqueueInClassRequest { UserId = userId, ClassId = classId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new EnqueueInClassResult
        {
            WasAlreadyEnqueued = reply.WasAlreadyEnqueued,
            StudentsQueue = reply.StudentsQueue,
            ClassName = reply.ClassName,
            ClassesDateTime = DateTimeOffset.FromUnixTimeSeconds(reply.ClassDateUnixTimestamp).DateTime
        });
    }
    
    public async Task<Result<DequeueFromClassResult>> DequeueFromClass(int classId, long userId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.DequeueFromClassAsync(new DequeueFromClassRequest { UserId = userId, ClassId = classId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new DequeueFromClassResult
        {
            WasAlreadyDequeued = reply.WasAlreadyDequeuedFromClass,
            StudentsQueue = reply.StudentsQueue,
            ClassName = reply.ClassName,
            ClassesDateTime = DateTimeOffset.FromUnixTimeSeconds(reply.ClassDateUnixTimestamp).DateTime
        });
    }

    public async Task<Result<ViewClassQueueResult>> ViewClassQueue(int classId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.ViewQueueClassAsync(new ViewQueueClassRequest{ ClassId = classId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new ViewClassQueueResult
        {
            StudentsQueue = reply.StudentsQueue,
            ClassName = reply.ClassName,
            ClassesDateTime = DateTimeOffset.FromUnixTimeSeconds(reply.ClassDateUnixTimestamp).DateTime
        });
    }

    public async Task<Result> AddSubscriber(long userId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.AddSubscriberAsync(new AddSubscriberRequest { SubscriberId = userId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok();
    }

    public async Task<Result> DeleteSubscriber(long userId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.DeleteSubscriberAsync(new DeleteSubscriberRequest { SubscriberId = userId}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok();
    }

    public async Task<Result<IEnumerable<SubscriberInfo>>> GetSubscribers(CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetSubscribersAsync(new Empty(), cancellationToken: cancellationToken);

        if (reply.IsFailed) return Result.Fail(reply.ErrorMessage);

        List<SubscriberInfo> subscribers = [];
        foreach (var subscriber in reply.Subscribers.ToList())
            subscribers.Add(new SubscriberInfo { TelegramId = subscriber.UserId, GroupId = subscriber.GroupId });

        return subscribers;
    }
}