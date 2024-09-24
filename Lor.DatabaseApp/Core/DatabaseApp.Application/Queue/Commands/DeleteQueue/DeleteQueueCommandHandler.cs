using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteQueueCommand, Result>
{
    public async Task<Result> Handle(DeleteQueueCommand request, CancellationToken cancellationToken)
    {
        List<Domain.Models.Queue>? listQueue;
        
        if (request.UserId is null && request.ClassId is null)  //TODO: потестить!
        {
            foreach (var classId in request.OutdatedClassList!)
            {
                listQueue = await unitOfWork.QueueRepository.GetOutdatedQueueListByClassId(classId, cancellationToken);

                if (listQueue is null) return Result.Fail("Очередь не найдена");
            
                foreach (var queue in listQueue)
                {
                    unitOfWork.QueueRepository.Delete(queue);
                }
            }
        }
        else if (request.OutdatedClassList is null)
        {
            listQueue = await unitOfWork.QueueRepository.GetOutdatedQueueListByClassId(request.ClassId!.Value, cancellationToken);
            
            if (listQueue is null) return Result.Fail("Очередь не найдена");
        
            Domain.Models.Queue queue = listQueue.First(x => x.UserId == request.UserId);
        
            unitOfWork.QueueRepository.Delete(queue);
        }

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        return Result.Ok();
    }
}