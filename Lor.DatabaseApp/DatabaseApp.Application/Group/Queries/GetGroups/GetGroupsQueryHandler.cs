using DatabaseApp.Application.Common.Converters;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroups;

public class GetGroupsQueryHandler(IUnitOfWork unitOfWork, GroupDto groupDto)
    : IRequestHandler<EmptyRequest, Result>
{
    public async Task<Result> Handle(EmptyRequest request, CancellationToken cancellationToken)
    {
        Dictionary<int, string>? groups = await unitOfWork.GroupRepository.GetGroups(cancellationToken);

        if (groups is null) return Result.Fail("Группы не найдены.");

        await groupDto.Handle(groups);

        return Result.Ok();
    }
}