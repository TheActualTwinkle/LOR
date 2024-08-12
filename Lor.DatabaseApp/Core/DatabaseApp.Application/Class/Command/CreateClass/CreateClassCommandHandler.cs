using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public class CreateClassCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateClassCommand, Result>
{
    public async Task<Result> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);
        
        if (group is null) return Result.Fail("Группа не найдена.");

        bool classExist = await unitOfWork.ClassRepository.CheckClass(request.ClassName, request.Date, cancellationToken);

        if (classExist) return Result.Fail("Такая пара уже существует.");

        Domain.Models.Class className = new()
        {
            GroupId = group.Id,
            Name = request.ClassName,
            Date = request.Date
        };

        await unitOfWork.ClassRepository.AddAsync(className, cancellationToken);

        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken), cancellationToken);

        return Result.Ok();
    }
}