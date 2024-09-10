using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotApp.Domain.Models;

public interface ITelegramBot
{
    void StartReceiving(CancellationToken cancellationToken = default);
    
    Task SendMessageAsync(long telegramId, string message, IReplyMarkup replyMarkup, CancellationToken cancellationToken = default);
    
    Task<User> GetMeAsync();
}