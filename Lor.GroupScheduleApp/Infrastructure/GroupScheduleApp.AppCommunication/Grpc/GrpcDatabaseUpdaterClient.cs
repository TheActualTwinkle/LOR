using DatabaseApp.AppCommunication.Grpc;
using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.Shared;
using Grpc.Net.Client;

namespace GroupScheduleApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterClient(string serviceUrl) : IDatabaseUpdaterCommunicationClient
{
    private DatabaseUpdater.DatabaseUpdaterClient? _client;
    
    public async Task StartAsync()
    {
        Console.WriteLine("Connecting to the gRPC service...");
        GrpcChannel channel = GrpcChannel.ForAddress(serviceUrl);
        await channel.ConnectAsync();
        Console.WriteLine("Successfully connected to the gRPC service.");
        
        _client = new DatabaseUpdater.DatabaseUpdaterClient(channel);
    }

    public Task StopAsync()
    {
        Console.WriteLine("Disconnecting from the gRPC service...");
        _client = null;
        Console.WriteLine("Successfully disconnected from the gRPC service.");
        return Task.CompletedTask;
    }

    public async Task SetAvailableGroups(IEnumerable<string> availableGroupNames)
    {
        await _client!.SetAvailableGroupsAsync(new SetAvailableGroupsRequest { GroupNames = { availableGroupNames } });
    }

    public async Task SetAvailableLabClasses(GroupClassesData groupClassesData)
    {
        Dictionary<string, long> classes = new();
        foreach (ClassData classData in groupClassesData.Classes)
        {
            DateTimeOffset dateTimeOffset = DateTime.SpecifyKind(classData.Date, DateTimeKind.Utc);
            classes.Add(classData.Name, dateTimeOffset.ToUnixTimeSeconds());
        }
        
        await _client!.SetAvailableLabClassesAsync(new SetAvailableLabClassesRequest
        {
            GroupName = groupClassesData.GroupName,
            Classes = { classes }
        });
    }
}