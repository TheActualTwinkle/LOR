namespace TelegramBotApp.AppCommunication.Data;

public readonly struct EnqueueInClassResult(IEnumerable<string> studentsQueue, string className, DateTime dateTime)
{
    public IEnumerable<string> StudentsQueue { get; } = studentsQueue;
    public string ClassName { get; } = className;
    public DateTime ClassesDateTime { get; } = dateTime;
}