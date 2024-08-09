using DatabaseApp.AppCommunication.Grpc;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.Shared;
using Grpc.Net.Client;

namespace GroupScheduleApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterClient(string serviceUrl) : IDatabaseUpdaterCommunicationClient
{
    private DatabaseUpdater.DatabaseUpdaterClient? _client;
    
    public async Task Start()
    {
        Console.WriteLine("Connecting to the Database...");
        GrpcChannel channel = GrpcChannel.ForAddress(serviceUrl);
        await channel.ConnectAsync();
        Console.WriteLine("Successfully connected to the Database.");
        
        _client = new DatabaseUpdater.DatabaseUpdaterClient(channel);
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