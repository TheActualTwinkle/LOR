using Telegram.Bot.Types;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBot
{
    void StartReceiving(IDatabaseCommunicationClient databaseCommunicator, CancellationToken cancellationToken);
    
    Task<User> GetMeAsync();
}