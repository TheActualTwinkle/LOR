using FluentResults;

namespace TelegramBotApp.AppCommunication.Interfaces;

public interface IDatabaseCommunicationClient : ICommunicationClient
{
    Task<Result> IsUserInGroup(long userId); 
    
    Task<Result<Dictionary<int, string>>> GetAvailableGroups();
    Task<Result<Dictionary<int, string>>> GetAvailableLabClasses(long userId);
    
    Task<Result<string>> TrySetGroup(long userId, string groupString);
    Task<Result<IEnumerable<string>>> EnqueueInClass(int cassId, long userId);
}