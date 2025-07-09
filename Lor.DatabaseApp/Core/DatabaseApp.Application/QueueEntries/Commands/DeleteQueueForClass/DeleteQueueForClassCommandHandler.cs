using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;

public class DeleteQueueForClassCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<DeleteQueueForClassCommand, Result>
{
    public async Task<Result> Handle(DeleteQueueForClassCommand request, CancellationToken cancellationToken)
    {
        var queueEntryRepository = unitOfWork.GetRepository<IQueueEntryRepository>();
        
        var outdatedQueueList = await queueEntryRepository.GetOutdatedQueueListByClassId(request.ClassId, cancellationToken);
        
        if (outdatedQueueList is null) return Result.Fail($"Очередь для {request.ClassId} не найдена");
        
        foreach (var queue in outdatedQueueList)
            queueEntryRepository.Delete(queue);

        var queues = (await queueEntryRepository.GetQueueByClassId(request.ClassId, cancellationToken)).Adapt<List<QueueEntryDto>>();
            
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, queues, cancellationToken: cancellationToken);
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}