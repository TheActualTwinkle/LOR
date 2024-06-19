using FluentResults;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramCommand
{
    string Command { get; }
    string Description { get; }

    Task<ExecutionResult> Execute(long chatId,
        TelegramCommandFactory telegramCommandFactory,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken);
}