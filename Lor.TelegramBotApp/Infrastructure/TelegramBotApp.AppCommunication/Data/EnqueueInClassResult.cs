namespace TelegramBotApp.AppCommunication.Data;

public record EnqueueInClassResult : ViewQueueClassResult
{
    public bool WasAlreadyEnqueued { get; init; }
}