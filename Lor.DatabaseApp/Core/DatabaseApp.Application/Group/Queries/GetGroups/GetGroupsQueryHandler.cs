using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroups;

public class GetGroupsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<EmptyRequest, Result<GroupDto>>
{
    public async Task<Result<GroupDto>> Handle(EmptyRequest request, CancellationToken cancellationToken)
    {
        Dictionary<int, string>? groups = await unitOfWork.GroupRepository.GetGroups(cancellationToken);

        if (groups is null) return Result.Fail("Группы не найдены.");

        GroupDto groupDto = new GroupDto() { GroupList = groups };

        return Result.Ok(groupDto);
    }
}