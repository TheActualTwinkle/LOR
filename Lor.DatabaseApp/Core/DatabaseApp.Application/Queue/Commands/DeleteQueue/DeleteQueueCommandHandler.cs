using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteQueueCommand, Result>
{
    public async Task<Result> Handle(DeleteQueueCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.OutdatedClassList)
        {
            List<Domain.Models.Queue>? listQueue = await unitOfWork.QueueRepository.GetOutdatedQueueListByClassId(item, cancellationToken);

            if (listQueue is null) return Result.Fail("Очередь не найдена");
            
            foreach (Domain.Models.Queue queue in listQueue)
            {
                unitOfWork.QueueRepository.Delete(queue);
            }
        }
        
        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken), cancellationToken);
        
        return Result.Ok();
    }
}