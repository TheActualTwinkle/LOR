namespace TelegramBotApp.AppCommunication.Consumers.Data;

public record TestMessage
{
    public required IEnumerable<Class> Classes { get; init; }
}

public record Class
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required DateOnly Date { get; set; }
}