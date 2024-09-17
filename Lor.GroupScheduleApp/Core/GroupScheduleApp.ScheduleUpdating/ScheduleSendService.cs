﻿using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.Shared;
using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.ScheduleUpdating.Settings;

namespace GroupScheduleApp.ScheduleUpdating;

public class ScheduleSendService(IScheduleProvider scheduleProvider, IDatabaseUpdaterCommunicationClient databaseUpdaterCommunicationClient, ScheduleSendServiceSettings settings) : IScheduleSendService
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
                Console.WriteLine(e.Message);
            }

            await Task.Delay(settings.SendInterval);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private async Task SendAllDataAsync()
    {
        IEnumerable<string> availableGroups = await scheduleProvider.GetAvailableGroupsAsync();
        IEnumerable<GroupClassesData> groupClassesData = await scheduleProvider.GetGroupClassesDataAsync();

        await databaseUpdaterCommunicationClient.SetAvailableGroups(availableGroups);

        foreach (GroupClassesData classesData in groupClassesData)
        {
            await databaseUpdaterCommunicationClient.SetAvailableLabClasses(classesData);
        }

        Console.WriteLine("Groups and classes data sent to database.");
    }
}