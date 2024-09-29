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
        cancellationToken =new CancellationToken();
        
        List<Domain.Models.Queue>? queueOfClass = await unitOfWork.QueueRepository.GetQueueByClassId(request.ClassId, cancellationToken);
            
        if (queueOfClass is null) return Result.Fail("Запись в очереди не найдена");

        uint userQueueNum = await unitOfWork.QueueRepository.GetUserQueueNum(request.TelegramId, request.ClassId, cancellationToken);
            
        Domain.Models.Queue queue = queueOfClass.First(x => x.QueueNum == userQueueNum);
        
        unitOfWork.QueueRepository.Delete(queue);
        
        List<Domain.Models.Queue>? queueAfterDelete = queueOfClass.Where(x => x.QueueNum > userQueueNum).ToList();

        foreach (var item in queueAfterDelete)
        {
            item.QueueNum = item.QueueNum-1;
            
            unitOfWork.QueueRepository.Update(item);
        }
        
        List<QueueDto> newQueue = mapper.From(queueOfClass.Where(x => x.QueueNum != userQueueNum).ToList()).AdaptToType<List<QueueDto>>();
        
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, newQueue, cancellationToken: cancellationToken);
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}