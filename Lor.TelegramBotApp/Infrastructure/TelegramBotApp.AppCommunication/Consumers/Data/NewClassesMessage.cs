namespace TelegramBotApp.AppCommunication.Consumers.Data;

public record NewClassesMessage
{
    public required int GroupId { get; init; }
    
    public required IEnumerable<Class> Classes { get; init; }
}

public record Class
{
    public required string Name { get; init; }
    public required DateOnly Date { get; init; }
}