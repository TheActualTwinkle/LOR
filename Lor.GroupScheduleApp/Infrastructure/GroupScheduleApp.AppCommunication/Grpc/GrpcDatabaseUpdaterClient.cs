using DatabaseApp.AppCommunication.Grpc;
using Google.Protobuf.Collections;
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
        List<string> classNames = groupClassesData.Classes.Select(d => d.Name).ToList();
        // ReSharper disable once UseCollectionExpression
        RepeatedField<string> repeatedField = new() { classNames };


        await _client!.SetAvailableLabClassesAsync(new SetAvailableLabClassesRequest { GroupName = groupClassesData.GroupName, ClassNames = { repeatedField }});
    }
}