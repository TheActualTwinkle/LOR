using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
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

    public async Task<Result<UserDto>> GetUserInfo(long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetUserInfoAsync(new GetUserInfoRequest { TelegramId = telegramId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail("Вы не авторизованы. Для авторизации введите /auth") : Result.Ok(new UserDto { FullName = reply.FullName, GroupName = reply.GroupName });
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetAvailableGroupsAsync(new Empty(), cancellationToken: cancellationToken);
        var groups = reply.IdGroupsMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        
        return Result.Ok(groups);
    }

    public async Task<Result<IEnumerable<ClassDto>>> GetAvailableLabClasses(long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetAvailableClassesAsync(new GetAvailableClassesRequest { TelegramId = telegramId }, cancellationToken: cancellationToken);
        
        if (reply.IsFailed) return Result.Fail(reply.ErrorMessage);

        return Result.Ok(reply.ClassInformation.Select(x => new ClassDto
        {
            Id = x.Id,
            Name = x.Name,
            Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(x.ClassDateUnixTimestamp).DateTime)
        }));
    }

    public async Task<Result<string>> SetGroup(long telegramId, string groupName, string fullName, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.SetGroupAsync(new SetGroupRequest { TelegramId = telegramId, GroupName = groupName, FullName = fullName}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok($"{reply.FullName}: группа {reply.GroupName} успешно установлена!");
    }

    public async Task<Result<EnqueueInClassDto>> EnqueueInClass(int classId, long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.EnqueueInClassAsync(new EnqueueInClassRequest { TelegramId = telegramId, ClassId = classId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new EnqueueInClassDto
        {
            Name = reply.Class.Name,
            Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(reply.Class.ClassDateUnixTimestamp).DateTime),
            StudentsQueue = reply.StudentsQueue,
            WasAlreadyEnqueued = reply.WasAlreadyEnqueued
        });
    }
    
    public async Task<Result<DequeueFromClassDto>> DequeueFromClass(int classId, long telegramId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.DequeueFromClassAsync(new DequeueFromClassRequest { TelegramId = telegramId, ClassId = classId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new DequeueFromClassDto
        {
            Name = reply.Class.Name,
            Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(reply.Class.ClassDateUnixTimestamp).DateTime),
            StudentsQueue = reply.StudentsQueue,
            WasAlreadyDequeuedFromClass = reply.WasAlreadyDequeuedFromClass
        });
    }

    public async Task<Result<ViewClassQueueDto>> ViewClassQueue(int classId, CancellationToken cancellationToken = default)
    {
        var reply = await _client!.ViewQueueClassAsync(new ViewQueueClassRequest{ ClassId = classId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new ViewClassQueueDto
        {
            Name = reply.Class.Name,
            Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(reply.Class.ClassDateUnixTimestamp).DateTime),
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

    public async Task<Result<IEnumerable<SubscriberDto>>> GetSubscribers(CancellationToken cancellationToken = default)
    {
        var reply = await _client!.GetSubscribersAsync(new Empty(), cancellationToken: cancellationToken);

        if (reply.IsFailed) 
            return Result.Fail(reply.ErrorMessage);

        List<SubscriberDto> subscribers = [];
        
        foreach (var subscriber in reply.Subscribers.ToList())
            subscribers.Add(new SubscriberDto { TelegramId = subscriber.TelegramId, GroupName = subscriber.GroupName });

        return subscribers;
    }
}