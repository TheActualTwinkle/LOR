namespace TelegramBotApp.AppCommunication.Data;

public record SubscriberInfo
{
    public required long TelegramId { get; init; }
    public required int GroupId { get; init; }
}