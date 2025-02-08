namespace TelegramBotApp.AppCommunication.Consumers.Data;

public record UpcomingClassesMessage
{
    public required string ClassName { get; init; }
    public required DateOnly ClassDate { get; init; }
    public required IEnumerable<long> UsersIds { get; init; }
}