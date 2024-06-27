using Telegram.Bot.Types;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Authorization.Interfaces;
using TelegramBotApp.Caching.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramBot
{
    void StartReceiving(IDatabaseCommunicationClient databaseCommunicator, IAuthorizationService authorizationService, ICacheService cacheService, CancellationToken cancellationToken);
    
    Task<User> GetMeAsync();
}