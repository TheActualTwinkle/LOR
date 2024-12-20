namespace TelegramBotApp.AppCommunication.Data;

public record EnqueueInClassResult : ViewClassQueueResult
{
    public bool WasAlreadyEnqueued { get; init; }
}