using DatabaseApp.AppCommunication.RemovalService.Interfaces;
using DatabaseApp.AppCommunication.RemovalService.Settings;
using DatabaseApp.Application.Class.Command.DeleteClasses;
using DatabaseApp.Application.Class.Queries;
using DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DatabaseApp.AppCommunication.RemovalService;

public class ClassRemovalService(
    ISender mediator,
    IRecurringJobManager recurringJobManager,
    ILogger<ClassRemovalService> logger,
    ClassRemovalServiceSettings settings) 
    : IClassRemovalService
{
    public async Task StartAsync(CancellationToken cancellationToken = default) 
    {
        await DeleteOutdatedClasses(cancellationToken);
        
        recurringJobManager.AddOrUpdate(
            "DeleteOutdatedClasses",
            "dba_queue",
            () => DeleteOutdatedClasses(cancellationToken),
            settings.CronExpression);
    }
    
    private async Task DeleteOutdatedClasses(CancellationToken cancellationToken = default)
    {
        var outdatedClassList = await mediator.Send(new GetOutdatedClassesQuery(), cancellationToken);

        if (outdatedClassList.IsSuccess &&
            outdatedClassList.Value.Count != 0)
        {
            await mediator.Send(new DeleteQueuesForClassesCommand
            {
                ClassesId = outdatedClassList.Value
            }, cancellationToken);

            await mediator.Send(new DeleteClassesCommand
            {
                ClassesId = outdatedClassList.Value
            }, cancellationToken);
        }
        
        logger.LogInformation("Outdated classes deleted");
    }
}