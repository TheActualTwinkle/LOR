using System.Globalization;
using DatabaseApp.AppCommunication.Common;
using DatabaseApp.AppCommunication.RemovalService.Interfaces;
using DatabaseApp.AppCommunication.RemovalService.Settings;
using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.DeleteClasses;
using DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DatabaseApp.AppCommunication.RemovalService;

public class ClassRemovalService(
    ISender mediator,
    IBackgroundJobClient backgroundJobClient,
    ILogger<ClassRemovalService> logger,
    ClassRemovalServiceSettings settings)
    : IClassRemovalService
{
    public Task ScheduleRemoval(
        IEnumerable<ClassDto> classesDto,
        CancellationToken cancellationToken = default)
    {
        foreach (var classDto in classesDto)
        {
            var classDate = classDto.Date.ToDateTime(TimeOnly.MinValue);
            
            var enqueueAt = new DateTimeOffset(
                classDate + settings.RemovalAdvanceTime, 
                TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));

            var jobId = backgroundJobClient.Schedule(
                "dba_queue",
                () => DeleteOutdatedClass(classDto, cancellationToken),
                enqueueAt);

            var executionTimeResult = ExecutionTimeProvider.GetNextExecutionTime(jobId);
            
            if (executionTimeResult.IsSuccess)
                logger.LogInformation("Class (classId: {classId}) removal job scheduled on: {time} UTC", 
                    classDto.Id, executionTimeResult.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            else
                logger.LogError("Class (classId: {classId}) removal job NOT scheduled! Error: {error}", 
                    classDto.Id, executionTimeResult.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        }

        return Task.CompletedTask;
    }

    // Used by Hangfire
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task DeleteOutdatedClass(ClassDto classDto, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeleteQueueForClassCommand
        {
            ClassId = classDto.Id
        }, cancellationToken);

        await mediator.Send(new DeleteClassCommand
        {
            ClassId = classDto.Id
        }, cancellationToken);
        
        logger.LogInformation("Outdated classes deleted");
    }
}