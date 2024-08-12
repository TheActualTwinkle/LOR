using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public class DeleteClassCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteClassCommand, Result>
{
    public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        List<Domain.Models.Class> classes = request.OutdatedClassList.Adapt<List<Domain.Models.Class>>();

        foreach (Domain.Models.Class item in classes)
        {
            unitOfWork.ClassRepository.Delete(item);

            await unitOfWork.SaveDbChangesAsync(cancellationToken);
        }
        
        return Result.Ok();
    }
}