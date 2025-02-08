using DatabaseApp.AppCommunication.ReminderService.Interfaces;
using DatabaseApp.AppCommunication.ReminderService.Settings;
using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Queries;
using DatabaseApp.Application.QueueEntries.Queries;
using DatabaseApp.Application.User.Queries;
using Hangfire;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using TelegramBotApp.AppCommunication.Consumers.Data;
using Class = DatabaseApp.AppCommunication.Consumers.Data.Class;

namespace DatabaseApp.AppCommunication.ReminderService;

public class ClassReminderService(
    IBus bus,
    ILogger<ClassReminderService> logger,
    ISender mediator,
    ClassReminderServiceSettings settings) : IClassReminderService
{
    public async Task ScheduleClassesNotification(IEnumerable<Class> classes, CancellationToken cancellationToken)
    {
        foreach (var classDto in classes)
        {
            var @class = await mediator.Send(new GetClassQuery
            {
                ClassName = classDto.Name,
                ClassDate = classDto.Date
            }, cancellationToken);
            
            if (@class.IsFailed)
            {
                logger.LogInformation(@class.Errors.First().Message);
                return;
            }
            
            var queue = await mediator.Send(new GetClassQueueQuery()
            {
                ClassId = @class.Value.Id
            }, cancellationToken);
            
            if (queue.IsFailed)
            {
                logger.LogInformation(queue.Errors.First().Message);
                return;
            }

            var users = await mediator.Send(new GetUsersFromQueueQuery
            {
                Queue = queue.Value
            }, cancellationToken);
            
            if (users.IsFailed)
            {
                logger.LogInformation(users.Errors.First().Message);
                return;
            }
            
            await ScheduleUpcomingClassNotificationJob(@class.Value, users.Value, cancellationToken);
        }
    }
    
    private Task ScheduleUpcomingClassNotificationJob(ClassDto @class, IEnumerable<long> usersIds, CancellationToken cancellationToken)
    {
        BackgroundJob.Schedule(
            () => PublishUpcomingClassesMessage(@class, usersIds, cancellationToken),
            DaysUntilClassRelativeNotification(@class.Date, int.Parse(settings.NotificationAdvanceTime)));
        return Task.CompletedTask;
    }

    private async Task PublishUpcomingClassesMessage(ClassDto classDto, IEnumerable<long> usersIds, CancellationToken cancellationToken = default)
    {
        UpcomingClassesMessage upcomingClassesMessage = new()
        {
            ClassName = classDto.Name,
            ClassDate = classDto.Date,
            UsersIds = usersIds
        };
        
        await bus.Publish(upcomingClassesMessage, cancellationToken);
    }

    private TimeSpan DaysUntilClassRelativeNotification(DateOnly classDate, int classNotificationTimeDays)
    {
        DateOnly days = DateOnly.FromDateTime(DateTime.Now.Date).AddDays(classNotificationTimeDays);
        return TimeSpan.FromDays(classDate.DayNumber - days.DayNumber);
    }
}