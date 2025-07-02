using System.Globalization;
using DatabaseApp.Application.Classes;
using DatabaseApp.Application.Classes.Command.DeleteClasses;
using DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;
using DatabaseApp.Application.Services.ReminderService.Common;
using DatabaseApp.Application.Services.RemovalService.Settings;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Services.RemovalService;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DatabaseApp.Application.Services.RemovalService;

public class ClassRemovalService(
    ISender mediator,
    IBackgroundJobClient backgroundJobClient,
    ILogger<ClassRemovalService> logger,
    ClassRemovalServiceSettings settings)
    : IClassRemovalService
{
    public Task ScheduleRemoval(
        IEnumerable<Class> classes,
        CancellationToken cancellationToken = default)
    {
        foreach (var @class in classes)
        {
            var classDate = @class.Date.ToDateTime(TimeOnly.MinValue);
            
            var enqueueAt = new DateTimeOffset(
                classDate + settings.RemovalAdvanceTime, 
                TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));

            var jobId = backgroundJobClient.Schedule(
                "dba_queue",
                () => DeleteOutdatedClass(@class, cancellationToken),
                enqueueAt);

            var executionTimeResult = ExecutionTimeProvider.GetNextExecutionTime(jobId);
            
            if (executionTimeResult.IsSuccess)
                logger.LogInformation("Class (classId: {classId}) removal job scheduled on: {time} UTC", 
                    @class.Id, executionTimeResult.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            else
                logger.LogError("Class (classId: {classId}) removal job NOT scheduled! Error: {error}", 
                    @class.Id, executionTimeResult.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        }

        return Task.CompletedTask;
    }

    // Used by Hangfire
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task DeleteOutdatedClass(Class @class, CancellationToken cancellationToken = default)
    {
        await mediator.Send(
            new DeleteQueueForClassCommand
            {
                ClassId = @class.Id
            },
            cancellationToken);

        await mediator.Send(
            new DeleteClassCommand
            {
                ClassId = @class.Id
            },
            cancellationToken);

        logger.LogInformation("Outdated classes deleted");
    }
}