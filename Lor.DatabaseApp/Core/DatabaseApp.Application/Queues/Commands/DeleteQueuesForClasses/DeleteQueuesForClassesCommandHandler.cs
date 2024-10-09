using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteOutdatedQueues;

public class DeleteQueuesForClassesCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<DeleteQueuesForClassesCommand, Result>
{
    public async Task<Result> Handle(DeleteQueuesForClassesCommand request, CancellationToken cancellationToken)
    {
        foreach (int classId in request.ClassesId)
        {
            List<Domain.Models.Queue>? outdatedQueueList = await unitOfWork.QueueRepository.GetOutdatedQueueListByClassId(classId, cancellationToken);
        
            if (outdatedQueueList is null) return Result.Fail($"Очередь для {classId} не найдена");
        
            foreach (Domain.Models.Queue queue in outdatedQueueList)
            {
                unitOfWork.QueueRepository.Delete(queue);
            }

            List<QueueDto> queues = mapper.From(await unitOfWork.QueueRepository.GetQueueByClassId(classId, cancellationToken)).AdaptToType<List<QueueDto>>();
            await cacheService.SetAsync(Constants.QueuePrefix + classId, queues, cancellationToken: cancellationToken);
        }

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}