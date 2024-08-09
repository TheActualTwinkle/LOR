namespace TelegramBotApp.AppCommunication;

public readonly struct EnqueueInClassResult(IEnumerable<string> studentsQueue, string className)
{
    public IEnumerable<string> StudentsQueue { get; } = studentsQueue;
    public string ClassName { get; } = className;
}