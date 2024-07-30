using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public class DeleteClassCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteClassCommand, Result>
{
    public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        foreach (Domain.Models.Class? item in request.OutdatedClassList)
        {
            unitOfWork.ClassRepository.Delete(item);

            await unitOfWork.SaveDbChangesAsync(cancellationToken);
        }
        
        return Result.Ok();
    }
}