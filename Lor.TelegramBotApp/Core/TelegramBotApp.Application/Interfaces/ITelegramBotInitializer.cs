using Microsoft.Extensions.Logging;
using Telegram.Bot.Polling;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Common;
using TelegramBotApp.Authorization.Interfaces;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBotInitializer
{
    ITelegramBot CreateBot(string token, ReceiverOptions receiverOptions, IDatabaseCommunicationClient databaseCommunicator, IAuthorizationService authorizationService, ILogger<TelegramBot> logger);     
    ReceiverOptions CreateReceiverOptions(); 
}