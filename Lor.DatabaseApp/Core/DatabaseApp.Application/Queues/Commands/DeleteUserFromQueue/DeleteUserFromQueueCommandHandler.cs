using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteUserFromQueueCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<DeleteUserFromQueueCommand, Result>
{
    public async Task<Result> Handle(DeleteUserFromQueueCommand request, CancellationToken cancellationToken)
    {
        var queueOfClass = await unitOfWork.QueueRepository.GetQueueByClassId(request.ClassId, cancellationToken);
            
        if (queueOfClass is null) return Result.Fail("Запись в очереди не найдена");

        var userQueueNum = await unitOfWork.QueueRepository.GetUserQueueNum(request.TelegramId, request.ClassId, cancellationToken);
            
        var queue = queueOfClass.First(x => x.QueueNum == userQueueNum);
        
        unitOfWork.QueueRepository.Delete(queue);
        
        var queueAfterDeletedEntry = queueOfClass.Where(x => x.QueueNum > userQueueNum);

        foreach (var item in queueAfterDeletedEntry)
        {
            item.QueueNum -= 1;
            
            unitOfWork.QueueRepository.Update(item);
        }

        queueOfClass.Remove(queue);
        
        var newQueue = mapper.From(queueOfClass).AdaptToType<List<QueueDto>>();
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, newQueue, cancellationToken: cancellationToken);
        
        return Result.Ok();
    }
}