using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramCommand
{
    string Command { get; }
    string Description { get; }
    
    string? ButtonDescriptionText { get; }

    Task<ExecutionResult> Execute(long chatId,
        TelegramCommandFactory factory,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken);
}