using DatabaseApp.AppCommunication.RemovalService.Interfaces;
using DatabaseApp.AppCommunication.RemovalService.Settings;
using DatabaseApp.Application.Class;
using DatabaseApp.Application.Class.Command.DeleteClasses;
using DatabaseApp.Application.Class.Queries;
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
            backgroundJobClient.Schedule(
                "dba_queue",
                () => DeleteOutdatedClasses(classDto, cancellationToken),
                classDto.Date.ToDateTime(TimeOnly.MinValue) + settings.RemovalAdvanceTime);
        }
        
        return Task.CompletedTask;
    }

private async Task DeleteOutdatedClasses(ClassDto classDto, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeleteQueuesForClassesCommand
        {
            ClassId = classDto.Id
        }, cancellationToken);

        await mediator.Send(new DeleteClassesCommand
        {
            ClassId = classDto.Id
        }, cancellationToken);
        
        logger.LogInformation("Outdated classes deleted");
    }
}