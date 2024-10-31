using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.ScheduleUpdating.Settings;
using Microsoft.Extensions.Logging;

namespace GroupScheduleApp.ScheduleUpdating;

public class ScheduleSendService(
    IScheduleProvider scheduleProvider,
    IDatabaseUpdaterCommunicationClient databaseUpdaterCommunicationClient,
    ScheduleSendServiceSettings settings,
    ILogger<ScheduleSendService> logger) : IScheduleSendService
{
    public async Task RunAsync()
    {
        while (true)
        {
            try
            {
                await SendAllDataAsync();
            }
            catch (Exception e)
            {
                logger.LogError("Error: {message}", e.Message);
            }

            await Task.Delay(settings.SendInterval);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private async Task SendAllDataAsync()
    {
        var availableGroups = await scheduleProvider.GetAvailableGroupsAsync();
        var groupClassesData = await scheduleProvider.GetGroupClassesDataAsync();

        await databaseUpdaterCommunicationClient.SetAvailableGroups(availableGroups);

        foreach (var classesData in groupClassesData)
            await databaseUpdaterCommunicationClient.SetAvailableLabClasses(classesData);

        logger.LogInformation("Groups and classes data sent to database.");
    }
}