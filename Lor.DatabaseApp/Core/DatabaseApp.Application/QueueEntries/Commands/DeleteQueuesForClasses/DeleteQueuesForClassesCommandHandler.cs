using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;

public class DeleteQueuesForClassesCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<DeleteQueuesForClassesCommand, Result>
{
    public async Task<Result> Handle(DeleteQueuesForClassesCommand request, CancellationToken cancellationToken)
    {
        foreach (var classId in request.ClassesId)
        {
            var outdatedQueueList = await unitOfWork.QueueEntryRepository.GetOutdatedQueueListByClassId(classId, cancellationToken);
        
            if (outdatedQueueList is null) return Result.Fail($"Очередь для {classId} не найдена");
        
            foreach (var queue in outdatedQueueList)
                unitOfWork.QueueEntryRepository.Delete(queue);

            var queues = mapper.From(await unitOfWork.QueueEntryRepository.GetQueueByClassId(classId, cancellationToken)).AdaptToType<List<QueueEntryDto>>();
            
            await cacheService.SetAsync(Constants.QueuePrefix + classId, queues, cancellationToken: cancellationToken);
        }

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}