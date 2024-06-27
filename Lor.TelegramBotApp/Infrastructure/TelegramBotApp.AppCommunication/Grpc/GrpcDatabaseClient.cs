using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public class GrpcDatabaseClient(string serviceUrl) : IDatabaseCommunicationClient
{
    private Database.DatabaseClient? _client;
    
    public Task Start()
    {
        GrpcChannel channel = GrpcChannel.ForAddress(serviceUrl);
        _client = new Database.DatabaseClient(channel);
            
        return Task.CompletedTask;
    }
    
    public async Task<Result<string>> GetUserGroup(long userId, CancellationToken cancellationToken = default)
    {
        GetUserGroupReply reply = await _client!.GetUserGroupAsync(new GetUserGroupRequest { UserId = userId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail("Вы не авторизованы. Для авторизации введите /auth <ФИО>") : Result.Ok(reply.GroupName);
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default)
    {
        GetAvailableGroupsReply reply = await _client!.GetAvailableGroupsAsync(new Empty(), cancellationToken: cancellationToken);
        Dictionary<int, string> groups = reply.IdGroupsMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        return Result.Ok(groups);
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableLabClasses(long userId, CancellationToken cancellationToken = default)
    {
        GetAvailableLabClassesReply reply = await _client!.GetAvailableLabClassesAsync(new GetAvailableLabClassesRequest { UserId = userId }, cancellationToken: cancellationToken);
        
        if (reply.IsFailed) return Result.Fail<Dictionary<int, string>>(reply.ErrorMessage);
        
        Dictionary<int, string> classes = reply.IdClassMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        return Result.Ok(classes);
    }

    public async Task<Result<string>> TrySetGroup(long userId, string groupName, CancellationToken cancellationToken = default)
    {
        TrySetGroupReply reply = await _client!.TrySetGroupAsync(new TrySetGroupRequest { UserId = userId, GroupName = groupName }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail(reply.ErrorMessage) : Result.Ok($"Группа {reply.GroupName} успешно установлена!");
    }

    public async Task<Result<IEnumerable<string>>> EnqueueInClass(int cassId, long userId, CancellationToken cancellationToken = default)
    {
        TryEnqueueInClassReply reply = await _client!.TryEnqueueInClassAsync(new TryEnqueueInClassRequest { UserId = userId, ClassId = cassId }, cancellationToken: cancellationToken);
        
        return reply.IsFailed ? Result.Fail<IEnumerable<string>>(reply.ErrorMessage) : Result.Ok<IEnumerable<string>>(reply.StudentsQueue);
    }
}