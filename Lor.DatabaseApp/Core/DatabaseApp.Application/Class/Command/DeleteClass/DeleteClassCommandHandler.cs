using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public class DeleteClassCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteClassCommand, Result>
{
    public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        foreach (int item in request.ClassesId)
        {
            Domain.Models.Class? @class = await unitOfWork.ClassRepository.GetClassById(item, cancellationToken);
            
            if (@class is null) return Result.Fail($"Пара {@class?.Name} не найдена.");
            
            unitOfWork.ClassRepository.Delete(@class);
        }
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}