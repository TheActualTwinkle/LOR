namespace DatabaseApp.Application.Services.ReminderService.Settings;

public record ClassReminderServiceSettings
{
    public required TimeSpan AdvanceNoticeTime { get; init; }
}