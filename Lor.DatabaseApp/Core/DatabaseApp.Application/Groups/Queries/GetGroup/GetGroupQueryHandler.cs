using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroup;

public class GetGroupQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetGroupQuery, Result<GroupDto>>
{
    public async Task<Result<GroupDto>> Handle(GetGroupQuery request, CancellationToken cancellationToken) =>
        await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken) is null
            ? Result.Fail("Группа не найдена")
            : Result.Ok(mapper.From(await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken)).AdaptToType<GroupDto>());
}