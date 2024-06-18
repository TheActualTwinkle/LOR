using Telegram.Bot.Types;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBot
{
    void StartReceiving(IDatabaseCommunicationClient databaseCommunicator, CancellationToken cancellationToken);

    Task SendMessageAsync(long telegramId, string message);
    
    Task<User> GetMeAsync();
}