namespace DatabaseApp.Application.Subscriber;

public record SubscriberDto
{
    public required long TelegramId { get; init; }
    public required int GroupId { get; init; }
}