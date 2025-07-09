using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Groups.Queries;

public class GetGroupQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetGroupQuery, Result<GroupDto>>
{
    public async Task<Result<GroupDto>> Handle(GetGroupQuery request, CancellationToken cancellationToken)
    {
        var groupRepository = unitOfWork.GetRepository<IGroupRepository>();
        
        var group = await groupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);
            
        return group == null 
            ? Result.Fail("Группа не найдена") 
            : Result.Ok(mapper.From(mapper.From(group)).AdaptToType<GroupDto>());
    }
}