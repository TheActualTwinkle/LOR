using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroups;

public class GetGroupsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetGroupsQuery, Result<List<GroupDto>>>
{
    public async Task<Result<List<GroupDto>>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Models.Group>? groups = await unitOfWork.GroupRepository.GetGroups(cancellationToken);

        if (groups is null || groups.Count == 0) return Result.Fail("Группы не найдены.");
        
        return Result.Ok(mapper.From(groups).AdaptToType<List<GroupDto>>());
    }
}