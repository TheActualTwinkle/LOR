using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Groups.Queries;

public class GetGroupQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetGroupQuery, Result<GroupDto>>
{
    public async Task<Result<GroupDto>> Handle(GetGroupQuery request, CancellationToken cancellationToken)
    {
        var group = await unitOfWork.GetRepository<IGroupRepository>().GetGroupByGroupName(request.GroupName, cancellationToken);
            
        return group == null 
            ? Result.Fail("Группа не найдена") 
            : Result.Ok(group.Adapt<GroupDto>());
    }
}