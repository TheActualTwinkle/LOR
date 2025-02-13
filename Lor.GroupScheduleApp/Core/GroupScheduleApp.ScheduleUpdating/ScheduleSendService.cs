using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.ScheduleUpdating.Settings;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace GroupScheduleApp.ScheduleUpdating;

public class ScheduleSendService(
    IScheduleProvider scheduleProvider,
    IDatabaseUpdaterCommunicationClient databaseUpdaterCommunicationClient,
    IRecurringJobManager recurringJobManager,
    ScheduleSendServiceSettings settings,
    ILogger<ScheduleSendService> logger) : IScheduleSendService
{
    public async Task StartAsync()
    {
        await SendAllDataAsync();
        
        recurringJobManager.AddOrUpdate(
            "SendAllData",
            "gsa_queue",
            () => SendAllDataAsync(),
            settings.CronExpression);
    }

    // Used by Hangfire
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task SendAllDataAsync()
    {
        var availableGroups = await scheduleProvider.GetAvailableGroupsAsync();
        var groupClassesData = await scheduleProvider.GetGroupClassesDataAsync();

        await databaseUpdaterCommunicationClient.SetAvailableGroups(availableGroups);

        foreach (var classesData in groupClassesData)
            await databaseUpdaterCommunicationClient.SetAvailableLabClasses(classesData);

        logger.LogInformation("Groups and classes data sent to database.");
    }
}