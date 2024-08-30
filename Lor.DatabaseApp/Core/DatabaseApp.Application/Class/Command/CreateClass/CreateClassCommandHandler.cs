using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public class CreateClassesCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateClassesCommand, Result>
{
    public async Task<Result> Handle(CreateClassesCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);
        
        if (group is null) return Result.Fail("Группа не найдена.");

        foreach (var item in request.Classes)
        {
            bool classExist = await unitOfWork.ClassRepository.CheckClass(item.Key, item.Value, cancellationToken);

            if (classExist) continue;
            
            Domain.Models.Class @class = new()
            {
                GroupId = group.Id,
                Name = item.Key,
                Date = item.Value
            };
            
            await unitOfWork.ClassRepository.AddAsync(@class, cancellationToken);
        }
        
        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken), cancellationToken);

        return Result.Ok();
    }
}