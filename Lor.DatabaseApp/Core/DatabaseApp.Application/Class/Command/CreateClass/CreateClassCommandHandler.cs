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

        Domain.Models.Class? className = new()
        {
            GroupId = group.Id,
            ClassName = request.ClassName,
            Date = request.Date
        };

        //TODO: прописать проверку на существование такой пары

        await unitOfWork.ClassRepository.AddAsync(className, cancellationToken);

        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken), cancellationToken);

        return Result.Ok();
    }
}