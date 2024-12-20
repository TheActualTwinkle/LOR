namespace TelegramBotApp.AppCommunication.Data;

public record ViewClassQueueResult
{
    public IEnumerable<string> StudentsQueue { get; init; }
    
    public string ClassName { get; init; }
    
    public DateTime ClassesDateTime { get; init; }
}