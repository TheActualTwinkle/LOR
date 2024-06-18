using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public class GrpcDatabaseCommunicationClient(string serviceUrl) : IDatabaseCommunicationClient
{
    private DatabaseCommunication.DatabaseCommunicationClient? _client;
    
    public Task Start()
    {
        GrpcChannel channel = GrpcChannel.ForAddress(serviceUrl);
        _client = new DatabaseCommunication.DatabaseCommunicationClient(channel);
            
        return Task.CompletedTask;
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableGroups()
    {
        GetAvailableGroupsReply reply = await _client!.GetAvailableGroupsAsync(new Empty());
        Dictionary<int, string> groups = reply.IdGroupsMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        return Result.Ok(groups);
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableLabClasses(long userId)
    {
        GetAvailableLabClassesReply reply = await _client!.GetAvailableLabClassesAsync(new GetAvailableLabClassesRequest { UserId = userId });
        Dictionary<int, string> classes = reply.IdClassMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        return Result.Ok(classes);
    }

    public async Task<Result<string>> TrySetGroup(long userId, string groupName)
    {
        TrySetGroupReply reply = await _client!.TrySetGroupAsync(new TrySetGroupRequest { UserId = userId, GroupName = groupName });
        
        return string.IsNullOrEmpty(reply.ErrorMessage) ? Result.Ok("Успешно установлена группа") : Result.Fail(reply.ErrorMessage);
    }

    public async Task<Result<IEnumerable<string>>> EnqueueInClass(int cassId, long userId)
    {
        TryEnqueueInClassReply reply = await _client!.TryEnqueueInClassAsync(new TryEnqueueInClassRequest { UserId = userId, ClassId = cassId });
        
        return string.IsNullOrEmpty(reply.ErrorMessage) ? Result.Ok<IEnumerable<string>>(reply.StudentsQueue) : Result.Fail<IEnumerable<string>>(reply.ErrorMessage);
    }
}