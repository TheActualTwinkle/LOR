using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBot
{
    void StartReceiving(IGroupScheduleCommunicator scheduleCommunicator, CancellationToken cancellationToken);

    Task SendMessageAsync(long telegramId, string message);
    
    Task<User> GetMeAsync();
}