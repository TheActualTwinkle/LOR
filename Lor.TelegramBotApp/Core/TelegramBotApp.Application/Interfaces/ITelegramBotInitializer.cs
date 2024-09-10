using Telegram.Bot.Polling;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Domain.Models;
using TelegramBotApp.Identity.Services.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBotInitializer
{
    ITelegramBot CreateBot(string token, ReceiverOptions receiverOptions,
        IDatabaseCommunicationClient databaseCommunicator, IRegistrationService registrationService,
        IAuthService authorizationService);

    ReceiverOptions CreateReceiverOptions();
}