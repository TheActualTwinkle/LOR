namespace TelegramBotApp.AppCommunication.Data;

public record EnqueueInClassDto : ViewClassQueueDto
{
    public required bool WasAlreadyEnqueued { get; init; }
}