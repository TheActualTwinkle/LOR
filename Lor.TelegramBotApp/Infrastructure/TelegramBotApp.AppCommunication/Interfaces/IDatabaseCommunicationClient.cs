using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using TelegramBotApp.AppCommunication.Data;

namespace TelegramBotApp.AppCommunication.Interfaces;

public interface IDatabaseCommunicationClient : ICommunicationClient
{
    Task<Result<Guid>> PreregisterUserAsync(string fullName, long telegramId, string email, string groupName,
        CancellationToken cancellationToken = default);

    Task<Result> CheckUserEmailStatusAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<Result<UserInfo>> GetUserInfoAsync(long userId, CancellationToken cancellationToken = default);

    Task<Result<Dictionary<int, string>>> GetAvailableGroupsAsync(CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<ClassInformation>>> GetAvailableLabClassesAsync(long userId,
        CancellationToken cancellationToken = default);

    Task<Result<string>> TrySetGroupAsync(long userId, string groupName, string fullName,
        CancellationToken cancellationToken = default);

    Task<Result<EnqueueInClassResult>> EnqueueInClassAsync(int cassId, long userId,
        CancellationToken cancellationToken = default);

    Task<Result> AddSubscriberAsync(long userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteSubscriberAsync(long userId, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SubscriberInfo>>> GetSubscribersAsync(CancellationToken cancellationToken = default);
}