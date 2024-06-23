using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;

namespace TelegramBotApp.Application.Interfaces;

public interface ICallbackQuery
{
    string Query { get; }
    
    Task<ExecutionResult> Execute(long chatId,
        IDatabaseCommunicationClient databaseCommunicator,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken);
}