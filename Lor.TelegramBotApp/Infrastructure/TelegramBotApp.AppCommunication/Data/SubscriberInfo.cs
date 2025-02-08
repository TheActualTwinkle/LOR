namespace TelegramBotApp.AppCommunication.Data;

public record SubscriberInfo
{
    public required long TelegramId { get; init; }
    public required string GroupName { get; init; }
}