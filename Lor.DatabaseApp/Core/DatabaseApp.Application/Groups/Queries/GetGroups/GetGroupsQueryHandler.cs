using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Groups.Queries;

public class GetGroupsQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<GetGroupsQuery, Result<List<GroupDto>>>
{
    public async Task<Result<List<GroupDto>>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        var cachedGroups = await cacheService.GetAsync<List<GroupDto>>(Constants.AvailableGroupsKey, cancellationToken);

        if (cachedGroups is not null) return Result.Ok(cachedGroups);

        var groupRepository = unitOfWork.GetRepository<IGroupRepository>();
        
        var groups = await groupRepository.GetGroups(cancellationToken);
        
        if (groups is null) return Result.Fail("Группы не найдены.");
        
        var groupsDto = groups.Adapt<List<GroupDto>>();
        
        await cacheService.SetAsync(Constants.AvailableGroupsKey, groupsDto, cancellationToken: cancellationToken);

        return Result.Ok(groupsDto);
    }
}