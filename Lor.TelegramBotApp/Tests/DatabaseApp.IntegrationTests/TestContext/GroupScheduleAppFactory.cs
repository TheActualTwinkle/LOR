using GroupScheduleApp.AppCommunication.Grpc;
using GroupScheduleApp.AppCommunication.Interfaces;

namespace DatabaseApp.Tests.TestContext;

public class GroupScheduleAppFactory
{
    public IDatabaseUpdaterCommunicationClient DatabaseUpdaterCommunicationClient { get; private set; } = null!;
    
    public async Task StartAsync()
    {
        DatabaseUpdaterCommunicationClient = new GrpcDatabaseUpdaterClient("http://localhost:31401");
        await DatabaseUpdaterCommunicationClient.StartAsync();
    }
    
    public async Task StopAsync()
    {
        await DatabaseUpdaterCommunicationClient.StopAsync();
    }
}