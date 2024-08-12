using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteQueueCommand, Result>
{
    public async Task<Result> Handle(DeleteQueueCommand request, CancellationToken cancellationToken)
    {
        List<Domain.Models.Class> classes = request.OutdatedClassList.Adapt<List<Domain.Models.Class>>();
        
        foreach (Domain.Models.Class item in classes)
        {
            List<Domain.Models.Queue>? listQueue = await unitOfWork.QueueRepository.GetOutdatedQueueListByClassId(item.Id, cancellationToken);

            if (listQueue is null) return Result.Fail("Очередь не найдена");
            
            foreach (Domain.Models.Queue queue in listQueue)
            {
                unitOfWork.QueueRepository.Delete(queue);

                await unitOfWork.SaveDbChangesAsync(cancellationToken);
            }
        }

        return Result.Ok();
    }
}