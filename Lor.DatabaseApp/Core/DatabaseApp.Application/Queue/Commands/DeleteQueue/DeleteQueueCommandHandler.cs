using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<DeleteQueueCommand, Result>
{
    public async Task<Result> Handle(DeleteQueueCommand request, CancellationToken cancellationToken)
    {
        if (request.OutdatedClassList is not null)
        {
            foreach (var classId in request.OutdatedClassList!)
            {
                List<Domain.Models.Queue>? outdatedQueueList = await unitOfWork.QueueRepository.GetOutdatedQueueListByClassId(classId, cancellationToken);

                if (outdatedQueueList is null) return Result.Fail("Очередь не найдена");
            
                foreach (var queue in outdatedQueueList)
                {
                    unitOfWork.QueueRepository.Delete(queue);
                }
                
                await cacheService.SetAsync(Constants.QueuePrefix + classId, 
                    mapper.From(await unitOfWork.QueueRepository.GetQueueByClassId(classId, cancellationToken))
                        .AdaptToType<List<QueueDto>>(), cancellationToken: cancellationToken);
            }
            
        }
        else if (request.TelegramId is not null && request.ClassId is not null)
        {
            List<Domain.Models.Queue>? queueOfClass = await unitOfWork.QueueRepository.GetQueueByClassId(request.ClassId!.Value, cancellationToken);
            
            if (queueOfClass is null) return Result.Fail("Очередь не найдена");

            uint userQueueNum = await unitOfWork.QueueRepository.GetUserQueueNum(request.TelegramId.Value, request.ClassId!.Value, cancellationToken);
            
            Domain.Models.Queue queue = queueOfClass.First(x => x.QueueNum == userQueueNum);
        
            unitOfWork.QueueRepository.Delete(queue);
            
            await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId.Value, 
                mapper.From(queueOfClass.Select(x => x.QueueNum != userQueueNum))
                    .AdaptToType<List<QueueDto>>(), cancellationToken: cancellationToken);
        }

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}