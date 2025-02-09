namespace TelegramBotApp.AppCommunication.Data;

public record SubscriberDto
{
    public required long TelegramId { get; init; }
    public required string GroupName { get; init; }
}