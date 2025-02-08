namespace TelegramBotApp.AppCommunication.Data;

public record ViewClassQueueResult
{
    public required IEnumerable<string> StudentsQueue { get; init; }
}