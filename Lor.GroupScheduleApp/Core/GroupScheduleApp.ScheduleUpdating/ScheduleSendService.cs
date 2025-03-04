using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.ScheduleUpdating.Settings;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace GroupScheduleApp.ScheduleUpdating;

public class ScheduleSendService(
    IScheduleProvider scheduleProvider,
    IDatabaseUpdaterCommunicationClient databaseUpdaterCommunicationClient,
    IRecurringJobManager recurringJobManager,
    ScheduleSendServiceSettings settings,
    ILogger<ScheduleSendService> logger) : IScheduleSendService
{
    private const string SendAllDataJobId = "SendAllData";
    
    public Task StartAsync()
    {
        recurringJobManager.AddOrUpdateDynamic<IScheduleSendService>(
            SendAllDataJobId,
            s => s.SendAllDataAsync(),
            settings.CronExpression,
#pragma warning disable CS0618 // Type or member is obsolete
            new DynamicRecurringJobOptions { QueueName = "gsa_queue" });
#pragma warning restore CS0618 // Type or member is obsolete
        
        return Task.CompletedTask;
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