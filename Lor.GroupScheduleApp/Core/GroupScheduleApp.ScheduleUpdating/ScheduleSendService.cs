using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.Shared;
using GroupScheduleApp.AppCommunication.Interfaces;

namespace GroupScheduleApp.ScheduleUpdating;

public class ScheduleSendService(IScheduleProvider scheduleProvider, IDatabaseUpdaterCommunicationClient databaseUpdaterCommunicationClient) : IScheduleSenderService
{
    public async Task Run()
    {
        // TODO: Сделать нормальный цикл отправления данных
        while (true)
        {
            await SendAllDataAsync();
            await Task.Delay(TimeSpan.FromDays(1));   
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