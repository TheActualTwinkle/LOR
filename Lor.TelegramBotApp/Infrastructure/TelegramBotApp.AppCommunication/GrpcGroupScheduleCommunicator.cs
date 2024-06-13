using FluentResults;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using GroupScheduleApp.Grpc;
using Grpc.Net.Client;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public class GrpcGroupScheduleCommunicator(string serviceUrl) : IGroupScheduleCommunicator
{
    private GroupSchedule.GroupScheduleClient? _client;
    private GrpcChannel? _channel;

    public Task Start()
    {
        _channel = GrpcChannel.ForAddress(serviceUrl);
        _client = new GroupSchedule.GroupScheduleClient(_channel);
            
        return Task.CompletedTask;
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableGroups()
    {
        if (_client == null)
        {
            throw new NullReferenceException("GroupScheduleClient is not initialized");
        }
        
        MapField<int, string>? idGroupsMap = (await _client.GetAvailableGroupsAsync(new Empty())).IdGroupsMap;
        if (idGroupsMap == null)
        {
            return Result.Fail("Не удалось получить доступные группы");
        }
        
        Dictionary<int, string> groups = idGroupsMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        return Result.Ok(groups);
    }

    public async Task<Result<Dictionary<int, string>>> GetAvailableLabClasses(int groupId)
    {
        if (_client == null)
        {
            throw new NullReferenceException("GroupScheduleClient is not initialized");
        }
        
        GetAvailableLabClassesRequest request = new()
        {
            GroupId = groupId
        };

        MapField<int, string>? idClassMap = (await _client.GetAvailableLabClassesAsync(request)).IdClassMap;
        if (idClassMap == null)
        {
            return Result.Fail("Не удалось получить доступные лаб. занятия для вашей группы");
        }
        
        Dictionary<int, string> classes = idClassMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        return Result.Ok(classes);
    }
}