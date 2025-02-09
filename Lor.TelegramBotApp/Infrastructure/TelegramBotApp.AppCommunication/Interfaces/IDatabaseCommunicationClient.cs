using FluentResults;
using TelegramBotApp.AppCommunication.Data;

namespace TelegramBotApp.AppCommunication.Interfaces;

public interface IDatabaseCommunicationClient : ICommunicationClient
{
    Task<Result<UserDto>> GetUserInfo(long telegramId, CancellationToken cancellationToken = default); 
    
    Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<ClassDto>>> GetAvailableLabClasses(long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result<string>> SetGroup(long telegramId, string groupName, string fullName, CancellationToken cancellationToken = default);
    
    Task<Result<EnqueueInClassDto>> EnqueueInClass(string className,  DateOnly classDate, long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result<DequeueFromClassDto>> DequeueFromClass(string className,  DateOnly classDate, long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result<ViewClassQueueDto>> ViewClassQueue(string className,  DateOnly classDate, CancellationToken cancellationToken = default);
    
    Task<Result> AddSubscriber(long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result> DeleteSubscriber(long telegramId, CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<SubscriberDto>>> GetSubscribers(CancellationToken cancellationToken = default);
}