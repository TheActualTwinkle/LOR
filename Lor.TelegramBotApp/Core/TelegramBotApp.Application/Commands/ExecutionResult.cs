using FluentResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotApp.Application.Commands;

public readonly struct ExecutionResult(Result<string> result, IReplyMarkup? replyMarkup = null)
{
    public Result<string> Result { get; } = result;
    public IReplyMarkup? ReplyMarkup { get; } = replyMarkup;
}