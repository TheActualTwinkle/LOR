using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories;

namespace TelegramBotApp.Application.Interfaces;

public interface ICallbackQuery
{
    string Query { get; }
    
    Task<ExecutionResult> Execute(long chatId,
        TelegramCommandQueryFactory factory,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken);
}