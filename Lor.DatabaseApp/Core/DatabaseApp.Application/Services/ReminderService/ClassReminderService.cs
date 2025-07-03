using System.Diagnostics;
using System.Globalization;
using DatabaseApp.Application.QueueEntries.Queries;
using DatabaseApp.Application.Services.ReminderService.Common;
using DatabaseApp.Application.Services.ReminderService.Settings;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Services.ReminderService;
using Hangfire;
using Lor.Shared.Messaging.Models;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace DatabaseApp.Application.Services.ReminderService;

public class ClassReminderService(
    IBus bus,
    ISender mediator,
    IBackgroundJobClient backgroundJobClient,
    ILogger<ClassReminderService> logger,
    ClassReminderServiceSettings settings)
    : IClassReminderService
{
    public static readonly ActivitySource ActivitySource = new("DomainServices.ClassReminderService");

    public Task ScheduleNotification(
        IEnumerable<Class> classes,
        CancellationToken cancellationToken = default)
    {
        var classesList = classes.ToList();

        var expiredClasses = GetExpiredClasses(classesList);

        foreach (var @class in expiredClasses)
            logger.LogWarning(
                "Skipping notify expired class (classId: {classId}, cassName: {classname})",
                @class.Id, @class.Name);

        var notExpiredClasses = classesList.Except(expiredClasses);

        foreach (var @class in notExpiredClasses)
        {
            var classDate = @class.Date.ToDateTime(TimeOnly.MinValue);

            var enqueueAt = new DateTimeOffset(
                classDate - settings.AdvanceNoticeTime,
                TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));

            var jobId = backgroundJobClient.Schedule(
                "dba_queue",
                () => PublishClassesReminderMessage(@class, cancellationToken),
                enqueueAt);

            var executionTimeResult = ExecutionTimeProvider.GetNextExecutionTime(jobId);

            if (executionTimeResult.IsSuccess)
                logger.LogInformation(
                    "Class (classId: {classId}) reminder job scheduled on: {time} UTC",
                    @class.Id, executionTimeResult.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            else
                logger.LogError(
                    "Class (classId: {classId}) reminder job NOT scheduled! Error: {error}",
                    @class.Id, executionTimeResult.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        }

        return Task.CompletedTask;
    }

    // Used by Hangfire
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task PublishClassesReminderMessage(
        Class @class,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity();
        
        activity?.SetTag("class.id", @class.Id);
        activity?.SetTag("class.name", @class.Name);
        activity?.SetTag("class.date", @class.Date.ToString("O"));

        var queue = await mediator.Send(
            new GetClassQueueQuery
            {
                ClassId = @class.Id
            }, cancellationToken);

        if (queue.IsFailed)
        {
            logger.LogInformation("{msg}", queue.Errors.First().Message);

            return;
        }

        var users = await mediator.Send(
            new GetEnqueuedUsersQuery
            {
                Queue = queue.Value
            }, cancellationToken);

        if (users.IsFailed)
        {
            logger.LogInformation("{msg}", users.Errors.First().Message);

            return;
        }

        activity?.SetTag("recipients.count", users.Value.Count);

        UpcomingClassesMessage upcomingClassesMessage = new()
        {
            ClassName = @class.Name,
            ClassDate = @class.Date,
            Users = users.Value.Adapt<IEnumerable<UserModel>>()
        };

        await bus.Publish(upcomingClassesMessage, cancellationToken);

        logger.LogInformation("Classes reminder message was published");
    }

    private List<Class> GetExpiredClasses(IEnumerable<Class> classes) =>
        classes
            .Where(x => x.Date.ToDateTime(TimeOnly.MinValue) < DateTime.Now)
            .ToList();
}