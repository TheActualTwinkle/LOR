using FluentResults;
using TelegramBotApp.AppCommunication.Consumers.Data;
using TelegramBotApp.AppCommunication.Data;

namespace TelegramBotApp.AppCommunication.Interfaces;

public interface IDatabaseCommunicationClient : ICommunicationClient
{
    Task<Result<UserInfo>> GetUserInfo(long userId, CancellationToken cancellationToken = default); 
    
    Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<Class>>> GetAvailableLabClasses(long userId, CancellationToken cancellationToken = default);
    
    Task<Result<string>> SetGroup(long userId, string groupName, string fullName, CancellationToken cancellationToken = default);
    
    Task<Result<EnqueueInClassResult>> EnqueueInClass(int classId, long userId, CancellationToken cancellationToken = default);
    
    Task<Result<DequeueFromClassResult>> DequeueFromClass(int classId, long userId, CancellationToken cancellationToken = default);
    
    Task<Result<ViewClassQueueResult>> ViewClassQueue(int classId, CancellationToken cancellationToken = default);
    
    Task<Result> AddSubscriber(long userId, CancellationToken cancellationToken = default);
    
    Task<Result> DeleteSubscriber(long userId, CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<SubscriberInfo>>> GetSubscribers(CancellationToken cancellationToken = default);
}