using System.Globalization;
using DatabaseApp.AppCommunication.Common;
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
        var classesDtoList = classesDto.ToList();
        
        var expiredClasses = GetExpiredClasses(classesDtoList);

        foreach (var @class in expiredClasses)
            logger.LogWarning("Skipping expired class (classId: {classId}, cassName: {classname})",
                @class.Id, @class.Name);

        var notExpiredClasses = classesDtoList.Except(expiredClasses);

        foreach (var classDto in notExpiredClasses)
        {
            var jobId = backgroundJobClient.Schedule(
                "dba_queue",
                () => PublishClassesReminderMessage(classDto, cancellationToken),
                classDto.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime() - settings.AdvanceNoticeTime);

            var executionTimeResult = ExecutionTimeProvider.GetNextExecutionTime(jobId);
            
            if (executionTimeResult.IsSuccess)
                logger.LogInformation("Class (classId: {classId}) reminder job scheduled on: {time} UTC",
                    classDto.Id, executionTimeResult.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));            
            else
                logger.LogError("Class (classId: {classId}) reminder job NOT scheduled! Error: {error}", 
                    classDto.Id, executionTimeResult.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));        }
        
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
    
    
    private List<ClassDto> GetExpiredClasses(IEnumerable<ClassDto> classesDto) =>
        classesDto
            .Where(x => x.Date.ToDateTime(TimeOnly.MinValue) < DateTime.Now)
            .ToList();
}