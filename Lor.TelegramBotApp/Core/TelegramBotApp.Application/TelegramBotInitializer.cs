using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Authorization.Interfaces;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.Application;

public class TelegramBotInitializer : ITelegramBotInitializer
{
#pragma warning disable CA1822
    public ITelegramBot CreateBot(string token, ReceiverOptions receiverOptions, IDatabaseCommunicationClient databaseCommunicator, IAuthorizationService authorizationService)
#pragma warning restore CA1822
    {
        return new TelegramBot(new TelegramBotClient(token), receiverOptions, databaseCommunicator, authorizationService);
    }

#pragma warning disable CA1822
    public ReceiverOptions CreateReceiverOptions() =>
#pragma warning restore CA1822
        new()
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
        };
}