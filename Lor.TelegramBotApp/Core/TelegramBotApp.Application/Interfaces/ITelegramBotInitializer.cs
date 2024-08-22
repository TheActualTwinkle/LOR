using Telegram.Bot.Polling;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Authorization.Interfaces;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBotInitializer
{
    ITelegramBot CreateBot(string token, ReceiverOptions receiverOptions, IDatabaseCommunicationClient databaseCommunicator, IAuthorizationService authorizationService);     
    ReceiverOptions CreateReceiverOptions(); 
}