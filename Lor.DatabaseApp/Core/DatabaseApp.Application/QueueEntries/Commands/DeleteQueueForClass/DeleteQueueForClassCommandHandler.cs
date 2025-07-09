using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;

public class DeleteQueueForClassCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<DeleteQueueForClassCommand, Result>
{
    public async Task<Result> Handle(DeleteQueueForClassCommand request, CancellationToken cancellationToken)
    {
        var queueEntryRepository = unitOfWork.GetRepository<IQueueEntryRepository>();
        
        var outdatedQueueList = await queueEntryRepository.GetOutdatedQueueListByClassId(request.ClassId, cancellationToken);
        
        if (outdatedQueueList is null) return Result.Fail($"Очередь для {request.ClassId} не найдена");
        
        foreach (var queue in outdatedQueueList)
            queueEntryRepository.Delete(queue);

        var queues = mapper.From(await queueEntryRepository.GetQueueByClassId(request.ClassId, cancellationToken)).AdaptToType<List<QueueEntryDto>>();
            
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, queues, cancellationToken: cancellationToken);
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}