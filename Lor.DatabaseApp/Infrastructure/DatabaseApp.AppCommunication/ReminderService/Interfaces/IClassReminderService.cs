using DatabaseApp.AppCommunication.Consumers.Data;

namespace DatabaseApp.AppCommunication.ReminderService.Interfaces;

public interface IClassReminderService
{
    public Task ScheduleClassesNotification(IEnumerable<Class> classes, CancellationToken cancellationToken);
}