using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public class DeleteClassCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteClassCommand, Result>
{
    public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.OutdatedClassList)
        {
            unitOfWork.ClassRepository.Delete((await unitOfWork.ClassRepository.GetClassById(item, cancellationToken))!);
        }
        
        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken), cancellationToken);
        
        return Result.Ok();
    }
}