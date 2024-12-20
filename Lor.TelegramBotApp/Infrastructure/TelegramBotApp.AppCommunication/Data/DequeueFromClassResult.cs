namespace TelegramBotApp.AppCommunication.Data;

public record DequeueFromClassResult : ViewClassQueueResult
{
    public bool WasAlreadyDequeued { get; init; }
}