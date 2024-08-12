using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Command.CreateGroup;

public class CreateGroupCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateGroupCommand, Result>
{
    public async Task<Result> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.Group? groupName = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);

        if (groupName is not null) return Result.Fail("Группа уже существует.");

        Domain.Models.Group group = new()
        {
            Name = request.GroupName
        };

        await unitOfWork.GroupRepository.AddAsync(group, cancellationToken);

        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken), cancellationToken);

        return Result.Ok();
    }
}