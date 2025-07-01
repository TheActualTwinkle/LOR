using DatabaseApp.Domain.Models;

namespace DatabaseApp.Domain.Services.ReminderService;

public interface IClassReminderService
{
    public Task ScheduleNotification(IEnumerable<Class> classesDto, CancellationToken cancellationToken);
}