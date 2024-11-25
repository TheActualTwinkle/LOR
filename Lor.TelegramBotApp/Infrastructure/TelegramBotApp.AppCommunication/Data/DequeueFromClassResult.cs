namespace TelegramBotApp.AppCommunication.Data;

public record DequeueFromClassResult : ViewQueueClassResult
{
    public bool WasAlreadyDequeued { get; init; }
}