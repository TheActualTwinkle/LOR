namespace GroupScheduleApp.ScheduleUpdating.Settings;

public readonly struct ScheduleSendServiceSettings(TimeSpan sendInterval)
{
    public TimeSpan SendInterval { get; } = sendInterval;
}