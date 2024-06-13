using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public class GrpcDatabaseCommunicator : IDatabaseCommunicator
{
    public Task Start()
    {
        return Task.CompletedTask;
    }
}