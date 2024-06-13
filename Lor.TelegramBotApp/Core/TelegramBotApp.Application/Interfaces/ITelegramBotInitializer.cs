using Telegram.Bot.Polling;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBotInitializer
{
    ITelegramBot CreateBot(string token, ReceiverOptions receiverOptions);     
    ReceiverOptions CreateReceiverOptions(); 
}