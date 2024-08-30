using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Tests.TestContext;

namespace TelegramBotApp.Tests.CommunicationTests;

public class DatabaseTests
{
    private readonly DatabaseAppFactory _databaseAppFactory = new();
    private readonly TelegramBotAppFactory _telegramBotAppFactory = new();

    private IDatabaseCommunicationClient _databaseCommunication = null!;
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await _databaseAppFactory.StartAsync();
        await _telegramBotAppFactory.StartAsync();
        
        _databaseCommunication = _telegramBotAppFactory.DatabaseCommunicationClient;
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await _databaseAppFactory.ResetDatabaseAsync();
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _databaseAppFactory.StopAsync();
        await _telegramBotAppFactory.StopAsync();
    }

    [Test]
    public async Task Testik()
    {
        Assert.Pass();
        
       var result = await _databaseCommunication.GetAvailableGroups();
       Console.WriteLine(result.Value.Count);
    }
}