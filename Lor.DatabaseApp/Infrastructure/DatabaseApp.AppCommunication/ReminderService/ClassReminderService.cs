using DatabaseApp.AppCommunication.Messages;
using DatabaseApp.AppCommunication.ReminderService.Interfaces;
using DatabaseApp.AppCommunication.ReminderService.Settings;
using DatabaseApp.Application.Class;
using DatabaseApp.Application.QueueEntries.Queries;
using DatabaseApp.Application.User;
using Hangfire;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DatabaseApp.AppCommunication.ReminderService;

public class ClassReminderService(
    IBus bus,
    ISender mediator,
    IBackgroundJobClient backgroundJobClient,
    ILogger<ClassReminderService> logger,
    ClassReminderServiceSettings settings) 
    : IClassReminderService
{
    public async Task ScheduleNotification(
        IEnumerable<ClassDto> classesDto,
        CancellationToken cancellationToken)
    {
        foreach (var classDto in classesDto)
        {
            var queue = await mediator.Send(new GetClassQueueQuery
            {
                ClassId = classDto.Id
            }, cancellationToken);
            
            if (queue.IsFailed)
            {
                logger.LogInformation("{msg}", queue.Errors.First().Message);
                return;
            }
            
            var users = await mediator.Send(new GetEnqueuedUsersQuery
            {
                Queue = queue.Value
            }, cancellationToken);
            
            if (users.IsFailed)
            {
                logger.LogInformation("{msg}", users.Errors.First().Message);
                return;
            }
            
            backgroundJobClient.Schedule(
                () => PublishClassesReminderMessage(classDto, users.Value, cancellationToken),
                classDto.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime() - settings.AdvanceNoticeTime);
        }
    }
    
    private async Task PublishClassesReminderMessage(
        ClassDto classDto, 
        IEnumerable<UserDto> usersIds, 
        CancellationToken cancellationToken = default)
    {
        UpcomingClassesMessage upcomingClassesMessage = new()
        {
            ClassName = classDto.Name,
            ClassDate = classDto.Date,
            Users = usersIds
        };
        
        await bus.Publish(upcomingClassesMessage, cancellationToken);
    }
}