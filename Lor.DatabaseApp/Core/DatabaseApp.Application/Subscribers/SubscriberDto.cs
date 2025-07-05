namespace DatabaseApp.Application.Subscribers;

public record SubscriberDto
{
    public required long TelegramId { get; init; }
    public required string GroupName { get; init; }
}