using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using TelegramBotApp.AppCommunication.Data;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public class GrpcDatabaseClient(string serviceUrl) : IDatabaseCommunicationClient
{
    private Database.DatabaseClient? _client;
    
    public async Task StartAsync()
    {
        Console.WriteLine("Connecting to the gRPC service...");
        GrpcChannel channel = GrpcChannel.ForAddress(serviceUrl);
        await channel.ConnectAsync();
        Console.WriteLine("Successfully connected to the gRPC service.");
        
        _client = new Database.DatabaseClient(channel);
    }
    
    public async Task<Result<UserInfo>> GetUserInfo(long userId, CancellationToken cancellationToken = default)
    {
        GetUserInfoReply reply = await _client!.GetUserInfoAsync(new GetUserInfoRequest { UserId = userId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail("Вы не авторизованы. Для авторизации введите /auth") : Result.Ok(new UserInfo { FullName = reply.FullName, GroupName = reply.GroupName });
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default)
    {
        GetAvailableGroupsReply reply = await _client!.GetAvailableGroupsAsync(new Empty(), cancellationToken: cancellationToken);
        Dictionary<int, string> groups = reply.IdGroupsMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        return Result.Ok(groups);
    }

    public async Task<Result<IEnumerable<ClassInformation>>> GetAvailableLabClasses(long userId, CancellationToken cancellationToken = default)
    {
        GetAvailableLabClassesReply reply = await _client!.GetAvailableLabClassesAsync(new GetAvailableLabClassesRequest { UserId = userId }, cancellationToken: cancellationToken);
        
        if (reply.IsFailed) return Result.Fail(reply.ErrorMessage);

        return reply.ClassInformation.ToList();
    }

    public async Task<Result<string>> TrySetGroup(long userId, string groupName, string fullName, CancellationToken cancellationToken = default)
    {
        TrySetGroupReply reply = await _client!.TrySetGroupAsync(new TrySetGroupRequest { UserId = userId, GroupName = groupName, FullName = fullName}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok($"{reply.FullName}: группа {reply.GroupName} успешно установлена!");
    }

    public async Task<Result<EnqueueInClassResult>> EnqueueInClass(int cassId, long userId, CancellationToken cancellationToken = default)
    {
        TryEnqueueInClassReply reply = await _client!.TryEnqueueInClassAsync(new TryEnqueueInClassRequest { UserId = userId, ClassId = cassId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok(new EnqueueInClassResult
        {
            WasAlreadyEnqueued = reply.WasAlreadyEnqueued,
            StudentsQueue = reply.StudentsQueue,
            ClassName = reply.ClassName,
            ClassesDateTime = DateTimeOffset.FromUnixTimeSeconds(reply.ClassDateUnixTimestamp).DateTime
        });
    }

    public async Task<Result> AddSubscriber(long userId, CancellationToken cancellationToken = default)
    {
        AddSubscriberReply reply = await _client!.AddSubscriberAsync(new AddSubscriberRequest { SubscriberId = userId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok();
    }

    public async Task<Result> DeleteSubscriber(long userId, CancellationToken cancellationToken = default)
    {
        DeleteSubscriberReply reply = await _client!.DeleteSubscriberAsync(new DeleteSubscriberRequest { SubscriberId = userId}, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok();
    }

    public async Task<Result<IEnumerable<SubscriberInfo>>> GetSubscribers(CancellationToken cancellationToken = default)
    {
        GetSubscribersReply reply = await _client!.GetSubscribersAsync(new Empty(), cancellationToken: cancellationToken);

        if (reply.IsFailed) return Result.Fail(reply.ErrorMessage);

        List<SubscriberInfo> subscribers = [];
        foreach (SubscriberInformation subscriber in reply.Subscribers.ToList())
        {
            subscribers.Add(new SubscriberInfo { TelegramId = subscriber.UserId, GroupId = subscriber.GroupId });
        }

        return subscribers;
    }
}