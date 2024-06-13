using FluentResults;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramCommand
{
    string Command { get; }
    string Description { get; }
    IEnumerable<string> Arguments { get; }

    Task<Result<string>> Execute(long chatId,
        IGroupScheduleCommunicator scheduleCommunicator,
        CancellationToken cancellationToken);
}