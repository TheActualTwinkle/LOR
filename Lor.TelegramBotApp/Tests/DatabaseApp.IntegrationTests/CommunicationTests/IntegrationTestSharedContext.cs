using DatabaseApp.Tests.TestContext;
using GroupScheduleApp.AppCommunication.Interfaces;
using TelegramBotApp.AppCommunication.Interfaces;

namespace DatabaseApp.Tests.CommunicationTests;

[SetUpFixture]
public record IntegrationTestSharedContext
{
    public static GroupScheduleAppFactory ScheduleAppFactory { get; } = new();
    public static TelegramAppFactory TelegramAppFactory { get; } = new();
    public static DatabaseAppFactory DatabaseAppFactory { get; } = new();
    public static IDatabaseCommunicationClient DatabaseCommunication { get; set; } = null!;
    public static IDatabaseUpdaterCommunicationClient DatabaseUpdaterCommunicationClient { get; set; } = null!;

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