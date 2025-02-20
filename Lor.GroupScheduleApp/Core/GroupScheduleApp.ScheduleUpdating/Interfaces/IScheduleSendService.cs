namespace GroupScheduleApp.ScheduleUpdating;

public interface IScheduleSendService
{
    Task StartAsync();
    
    Task SendAllDataAsync();
}