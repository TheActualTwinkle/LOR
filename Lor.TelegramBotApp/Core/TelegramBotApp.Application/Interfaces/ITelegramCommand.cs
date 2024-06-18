using FluentResults;
using TelegramBotApp.Application.Factories;

namespace TelegramBotApp.Application.Interfaces;

public interface ITelegramCommand
{
    string Command { get; }
    string Description { get; }

    Task<Result<string>> Execute(long chatId,
        TelegramCommandFactory telegramCommandFactory,
        IReadOnlyCollection<string> arguments,
        CancellationToken cancellationToken);
}