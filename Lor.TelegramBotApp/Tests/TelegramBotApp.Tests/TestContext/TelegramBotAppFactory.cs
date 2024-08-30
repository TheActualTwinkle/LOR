using TelegramBotApp.AppCommunication;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.Tests.TestContext;

public class TelegramBotAppFactory
{
    public IDatabaseCommunicationClient DatabaseCommunicationClient { get; private set; } = null!;
    
    public async Task StartAsync()
    {
        DatabaseCommunicationClient = new GrpcDatabaseClient("http://localhost:31401");
        await DatabaseCommunicationClient.StartAsync();
    }
    
    public async Task StopAsync()
    {
        await DatabaseCommunicationClient.StopAsync();
    }
}