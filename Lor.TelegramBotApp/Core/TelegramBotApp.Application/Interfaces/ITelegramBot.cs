using Telegram.Bot.Types;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Authorization.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBot
{
    void StartReceiving(IDatabaseCommunicationClient databaseCommunicator, IAuthorizationService authorizationService, CancellationToken cancellationToken);
    
    Task<User> GetMeAsync();
}