using DatabaseApp.Application.Class;

namespace DatabaseApp.AppCommunication.ReminderService.Interfaces;

public interface IClassReminderService
{
    public Task ScheduleNotification(IEnumerable<ClassDto> classesDto, CancellationToken cancellationToken);
}