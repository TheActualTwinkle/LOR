using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.Shared;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Shared.GrpcServices;

namespace GroupScheduleApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterClient(string serviceUrl, ILogger<GrpcDatabaseUpdaterClient> logger) : IDatabaseUpdaterCommunicationClient
{
    private DatabaseUpdater.DatabaseUpdaterClient? _client;
    
    public async Task StartAsync()
    {
        logger.LogInformation("Connecting to the Database gRPC service...");
        var channel = GrpcChannel.ForAddress(serviceUrl);
        await channel.ConnectAsync();
        logger.LogInformation("Successfully connected to the Database gRPC service.");
        
        _client = new DatabaseUpdater.DatabaseUpdaterClient(channel);
    }

    public Task StopAsync()
    {
        logger.LogInformation("Disconnecting from the Database gRPC service...");
        _client = null;
        logger.LogInformation("Successfully disconnected from the Database gRPC service.");
        return Task.CompletedTask;
    }

    public async Task SetAvailableGroups(IEnumerable<string> availableGroupNames) =>
        await _client!.SetAvailableGroupsAsync(new SetAvailableGroupsRequest { GroupNames = { availableGroupNames } });

    public async Task SetAvailableLabClasses(GroupClassesData groupClassesData)
    {
        Dictionary<string, long> classes = new();
        foreach (var classData in groupClassesData.Classes)
        {
            DateTimeOffset dateTimeOffset = DateTime.SpecifyKind(classData.Date, DateTimeKind.Utc);
            classes.Add(classData.Name, dateTimeOffset.ToUnixTimeSeconds());
        }
        
        await _client!.SetAvailableClassesAsync(new SetAvailableClassesRequest
        {
            GroupName = groupClassesData.GroupName,
            Classes = { classes }
        });
    }
}