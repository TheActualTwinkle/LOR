using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramCommand
{
    string Command { get; }
    string Description { get; }

    Task<ExecutionResult> Execute(long chatId,
        IDatabaseCommunicationClient databaseCommunicator,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken);
}