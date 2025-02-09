namespace DatabaseApp.AppCommunication.ReminderService.Settings;

public record ClassReminderServiceSettings
{
    public required TimeSpan AdvanceNoticeTime { get; init; }
}