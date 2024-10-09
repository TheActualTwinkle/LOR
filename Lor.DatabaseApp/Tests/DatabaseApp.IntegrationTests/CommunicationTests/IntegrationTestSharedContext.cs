using DatabaseApp.Tests.TestContext;
using GroupScheduleApp.AppCommunication.Interfaces;
using TelegramBotApp.AppCommunication.Interfaces;

namespace DatabaseApp.Tests.CommunicationTests;

[SetUpFixture]
public class IntegrationTestSharedContext
{
    private static GroupScheduleAppFactory ScheduleAppFactory { get; } = new();
    private static TelegramAppFactory TelegramAppFactory { get; } = new();
    public static DatabaseAppFactory DatabaseAppFactory { get; } = new();
    
    public static IDatabaseCommunicationClient DatabaseCommunication { get; private set; } = null!;
    public static IDatabaseUpdaterCommunicationClient DatabaseUpdaterCommunicationClient { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        await DatabaseAppFactory.StartAsync();
        await ScheduleAppFactory.StartAsync();
        await TelegramAppFactory.StartAsync();

        DatabaseCommunication = TelegramAppFactory.DatabaseCommunicationClient;
        DatabaseUpdaterCommunicationClient = ScheduleAppFactory.DatabaseUpdaterCommunicationClient;

        await DatabaseCommunication.StartAsync();
        await DatabaseUpdaterCommunicationClient.StartAsync();
    }

    [OneTimeTearDown]
    public async Task Cleanup()
    {
        await Task.WhenAll(
            DatabaseAppFactory.StopAsync(), 
            ScheduleAppFactory.StopAsync(), 
            TelegramAppFactory.StopAsync());
    }
}