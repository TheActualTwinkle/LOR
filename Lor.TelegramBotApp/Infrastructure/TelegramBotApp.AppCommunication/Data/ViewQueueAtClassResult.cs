namespace TelegramBotApp.AppCommunication.Data;

public readonly struct ViewQueueAtClassResult
{
    public IEnumerable<string> StudentsQueue { get; init; }
    
    public string ClassName { get; init; }
    
    public DateTime ClassesDateTime { get; init; }
}