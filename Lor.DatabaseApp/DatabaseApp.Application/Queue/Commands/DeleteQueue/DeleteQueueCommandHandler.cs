using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteQueueCommand, Result>
{
    public async Task<Result> Handle(DeleteQueueCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.OutdatedClaasList)
        {
            List<Domain.Models.Queue>? listQueue = await unitOfWork.QueueRepository.GetOutdatedQueueListByClassId(item.Id, cancellationToken);

            if (listQueue is null) return Result.Fail("");
            
            foreach (var queue in listQueue)
            {
                unitOfWork.QueueRepository.Delete(queue);

                await unitOfWork.SaveDbChangesAsync(cancellationToken);
            }
        }

        return Result.Ok();
    }
}