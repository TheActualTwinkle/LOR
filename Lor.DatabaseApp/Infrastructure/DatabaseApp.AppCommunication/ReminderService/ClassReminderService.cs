using DatabaseApp.AppCommunication.Messages;
using DatabaseApp.AppCommunication.ReminderService.Interfaces;
using DatabaseApp.AppCommunication.ReminderService.Settings;
using DatabaseApp.Application.Class;
using DatabaseApp.Application.QueueEntries.Queries;
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
    public Task ScheduleNotification(
        IEnumerable<ClassDto> classesDto,
        CancellationToken cancellationToken = default)
    {
        foreach (var classDto in classesDto)
            backgroundJobClient.Schedule(
                "dba_queue",
                () => PublishClassesReminderMessage(classDto, cancellationToken),
                classDto.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime() - settings.AdvanceNoticeTime);
        
        return Task.CompletedTask;
    }
    
    // Used by Hangfire
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task PublishClassesReminderMessage(
        ClassDto classDto,
        CancellationToken cancellationToken = default)
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
        
        UpcomingClassesMessage upcomingClassesMessage = new()
        {
            ClassName = classDto.Name,
            ClassDate = classDto.Date,
            Users = users.Value
        };
        
        await bus.Publish(upcomingClassesMessage, cancellationToken);
        
        logger.LogInformation("Classes reminder message was published");
    }
}