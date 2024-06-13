using FluentResults;
using GroupScheduleApp.Grpc;

namespace TelegramBotApp.AppCommunication.Interfaces;

public interface IGroupScheduleCommunicator : IAppCommunicator
{
    Task<Result<Dictionary<int, string>>> GetAvailableGroups();
    Task<Result<Dictionary<int, string>>> GetAvailableLabClasses(int groupId);
}