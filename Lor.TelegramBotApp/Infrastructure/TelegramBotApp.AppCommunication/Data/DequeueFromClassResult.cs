namespace TelegramBotApp.AppCommunication.Data;

public readonly struct DequeueFromClassResult
{
    public bool WasAlreadyDequeued { get; init; }

    public IEnumerable<string> StudentsQueue { get; init; }
    
    public string ClassName { get; init; }
    
    public DateTime ClassesDateTime { get; init; }
}