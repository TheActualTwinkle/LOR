using DatabaseApp.AppCommunication.Consumers.Data;
using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using TelegramBotApp.AppCommunication.Data;
using TelegramBotApp.AppCommunication.Extensions;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.AppCommunication.Data;

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

    public async Task<Result<UserInfo>> GetUserInfo(long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetUserInfoAsync(new GetUserInfoRequest { TelegramId = telegramId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail("Вы не авторизованы. Для авторизации введите /auth") : Result.Ok(new UserInfo { FullName = reply.FullName, GroupName = reply.GroupName });
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetAvailableGroupsAsync(new Empty(), cancellationToken: cancellationToken);
        var groups = reply.IdGroupsMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        
        return Result.Ok(groups);
    }

    public async Task<Result<IEnumerable<Class>>> GetAvailableLabClasses(long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetAvailableClassesAsync(new GetAvailableClassesRequest { TelegramId = telegramId }, cancellationToken: cancellationToken);
        
        if (reply.IsFailed) return Result.Fail(reply.ErrorMessage);

        return Result.Ok(reply.ClassInformation.Select(x => new Class
        {
            Name = x.ClassName,
            Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(x.ClassDateUnixTimestamp).DateTime)
        }));
    }

    public async Task<Result<string>> SetGroup(long telegramId, string groupName, string fullName, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.SetGroupAsync(new SetGroupRequest { TelegramId = telegramId, GroupName = groupName, FullName = fullName}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok($"{reply.FullName}: группа {reply.GroupName} успешно установлена!");
    }

    public async Task<Result<EnqueueInClassResult>> EnqueueInClass(string className,  DateOnly classDate, long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.EnqueueInClassAsync(new EnqueueInClassRequest { TelegramId = telegramId, ClassName =  className, ClassDateUnixTimestamp = classDate.ToUnixTime()}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new EnqueueInClassResult
        {
            WasAlreadyEnqueued = reply.WasAlreadyEnqueued,
            StudentsQueue = reply.StudentsQueue
        });
    }
    
    public async Task<Result<DequeueFromClassResult>> DequeueFromClass(string className,  DateOnly classDate, long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.DequeueFromClassAsync(new DequeueFromClassRequest {  TelegramId = telegramId, ClassName =  className, ClassDateUnixTimestamp = classDate.ToUnixTime() }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new DequeueFromClassResult
        {
            WasAlreadyDequeued = reply.WasAlreadyDequeuedFromClass,
            StudentsQueue = reply.StudentsQueue
        });
    }

    public async Task<Result<ViewClassQueueResult>> ViewClassQueue(string className,  DateOnly classDate, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.ViewQueueClassAsync(new ViewQueueClassRequest{ ClassName = className, ClassDateUnixTimestamp = classDate.ToUnixTime()}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new ViewClassQueueResult
        {
            StudentsQueue = reply.StudentsQueue
        });
    }

    public async Task<Result> AddSubscriber(long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.AddSubscriberAsync(new AddSubscriberRequest { TelegramId = telegramId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok();
    }

    public async Task<Result> DeleteSubscriber(long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.DeleteSubscriberAsync(new DeleteSubscriberRequest { TelegramId = telegramId}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok();
    }

    public async Task<Result<IEnumerable<SubscriberInfo>>> GetSubscribers(CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetSubscribersAsync(new Empty(), cancellationToken: cancellationToken);

        if (reply.IsFailed) return Result.Fail(reply.ErrorMessage);

        List<SubscriberInfo> subscribers = [];
        foreach (var subscriber in reply.Subscribers.ToList())
            subscribers.Add(new SubscriberInfo { TelegramId = subscriber.TelegramId, GroupName = subscriber.GroupName });

        return subscribers;
    }
}