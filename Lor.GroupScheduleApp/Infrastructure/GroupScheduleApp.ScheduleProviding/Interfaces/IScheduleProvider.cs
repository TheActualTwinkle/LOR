using GroupScheduleApp.Shared;

namespace GroupScheduleApp.ScheduleProviding.Interfaces;

public interface IScheduleProvider
{
    Task<IEnumerable<string>> GetAvailableGroupsAsync();
    
    Task<IEnumerable<GroupClassesData>> GetGroupClassesDataAsync();
}