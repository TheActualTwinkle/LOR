namespace DatabaseApp.Application.Subscriber;

public record SubscriberDto()
{
    public required long TelegramId { get; set; }
    public required int GroupId { get; set; }
}