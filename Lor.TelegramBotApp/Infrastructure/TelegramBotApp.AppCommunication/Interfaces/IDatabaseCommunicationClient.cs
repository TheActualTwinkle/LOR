using FluentResults;
using TelegramBotApp.AppCommunication.Consumers.Data;
using TelegramBotApp.AppCommunication.Data;

namespace TelegramBotApp.AppCommunication.Interfaces;

public interface IDatabaseCommunicationClient : ICommunicationClient
{
    Task<Result<UserInfo>> GetUserInfo(long telegramId, CancellationToken cancellationToken = default); 
    
    Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<Class>>> GetAvailableLabClasses(long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result<string>> SetGroup(long telegramId, string groupName, string fullName, CancellationToken cancellationToken = default);
    
    Task<Result<EnqueueInClassResult>> EnqueueInClass(string className,  DateOnly classDate, long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result<DequeueFromClassResult>> DequeueFromClass(string className,  DateOnly classDate, long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result<ViewClassQueueResult>> ViewClassQueue(string className,  DateOnly classDate, CancellationToken cancellationToken = default);
    
    Task<Result> AddSubscriber(long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result> DeleteSubscriber(long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<SubscriberInfo>>> GetSubscribers(CancellationToken cancellationToken = default);
}