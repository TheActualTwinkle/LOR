using FluentResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotApp.Application.Commands;

public struct ExecutionResult
{
    public Result<string> Result { get; }
    public IReplyMarkup ReplyMarkup { get; }

    public ExecutionResult(Result<string> result, IReplyMarkup? replyMarkup = null)
    {
        replyMarkup ??= new ReplyKeyboardRemove();
        
        Result = result;
        ReplyMarkup = replyMarkup;
    }
}