namespace TelegramBotApp.AppCommunication.Data;

public record ViewClassQueueDto
{
    public required IEnumerable<string> StudentsQueue { get; init; }
}