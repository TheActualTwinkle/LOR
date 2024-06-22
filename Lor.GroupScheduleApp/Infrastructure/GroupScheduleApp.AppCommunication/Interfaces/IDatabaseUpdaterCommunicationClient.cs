using GroupScheduleApp.Shared;

namespace GroupScheduleApp.AppCommunication.Interfaces;

public interface IDatabaseUpdaterCommunicationClient : ICommunicationClient
{
    Task SetAvailableGroups(IEnumerable<string> availableGroupNames);
    Task SetAvailableLabClasses(GroupClassesData groupClassesData);
}