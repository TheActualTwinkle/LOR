namespace GroupScheduleApp.ScheduleUpdating.Settings;

public record ScheduleSendServiceSettings
{
    public required string CronExpression { get; init; }
};