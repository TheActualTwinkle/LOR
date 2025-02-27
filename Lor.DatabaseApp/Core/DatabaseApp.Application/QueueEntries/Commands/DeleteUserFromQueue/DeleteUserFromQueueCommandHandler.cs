using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteQueue;

public class DeleteUserFromQueueCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<DeleteUserFromQueueCommand, Result>
{
    public async Task<Result> Handle(DeleteUserFromQueueCommand request, CancellationToken cancellationToken)
    {
        var queueOfClass = await unitOfWork.QueueEntryRepository.GetQueueByClassId(request.ClassId, cancellationToken);
            
        if (queueOfClass is null) return Result.Fail("Запись в очереди не найдена");

        var userQueueNum = await unitOfWork.QueueEntryRepository.GetUserQueueNum(request.TelegramId, request.ClassId, cancellationToken);
            
        var queue = queueOfClass.First(x => x.QueueNum == userQueueNum);
        
        unitOfWork.QueueEntryRepository.Delete(queue);
        
        var queueAfterDeletedEntry = queueOfClass.Where(x => x.QueueNum > userQueueNum);

        foreach (var item in queueAfterDeletedEntry)
        {
            item.QueueNum -= 1;
            
            unitOfWork.QueueEntryRepository.Update(item);
        }

        queueOfClass.Remove(queue);
        
        var newQueue = mapper.From(queueOfClass).AdaptToType<List<QueueEntryDto>>();
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, newQueue, cancellationToken: cancellationToken);
        
        return Result.Ok();
    }
}