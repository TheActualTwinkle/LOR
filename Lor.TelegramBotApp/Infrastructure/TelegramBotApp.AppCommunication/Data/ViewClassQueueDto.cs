namespace TelegramBotApp.AppCommunication.Data;

public record ViewClassQueueDto
{
    public required string Name { get; init; }
    
    public required DateOnly Date { get; init; }
    
    public required IEnumerable<string> StudentsQueue { get; init; }
}