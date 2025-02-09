namespace TelegramBotApp.AppCommunication.Data;

public record DequeueFromClassDto : ViewClassQueueDto
{
    public required bool WasAlreadyDequeuedFromClass { get; init; }
}