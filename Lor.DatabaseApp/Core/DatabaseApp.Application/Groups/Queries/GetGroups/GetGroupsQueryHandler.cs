using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroups;

public class GetGroupsQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<GetGroupsQuery, Result<List<GroupDto>>>
{
    public async Task<Result<List<GroupDto>>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        List<GroupDto>? cachedGroups = await cacheService.GetAsync<List<GroupDto>>(Constants.AvailableGroupsKey, cancellationToken);

        if (cachedGroups is not null) return Result.Ok(cachedGroups);
        
        List<Domain.Models.Group>? groups = await unitOfWork.GroupRepository.GetGroups(cancellationToken);
        
        if (groups is null) return Result.Fail("Группы не найдены.");
        
        List<GroupDto> groupsDto = mapper.From(groups).AdaptToType<List<GroupDto>>();
        
        await cacheService.SetAsync(Constants.AvailableGroupsKey, groupsDto, cancellationToken: cancellationToken);

        return Result.Ok(groupsDto);
    }
}