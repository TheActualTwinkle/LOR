using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Groups.Command.CreateGroup;

public class CreateGroupsCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<CreateGroupsCommand, Result>
{
    public async Task<Result> Handle(CreateGroupsCommand request, CancellationToken cancellationToken)
    {
        var groupRepository = unitOfWork.GetRepository<IGroupRepository>();
        
        foreach (var item in request.GroupNames)
        {
            var isGroupExists = await groupRepository.GetGroupByGroupName(item, cancellationToken) == null;

            if (!isGroupExists)
                continue;

            Domain.Models.Group group = new()
            {
                Name = item
            };
                
            await groupRepository.AddAsync(group, cancellationToken);
        }
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        var groups = await groupRepository.GetGroups(cancellationToken);

        if (groups is null) return Result.Fail("Группы не найдены.");
        
        var groupDtos = mapper.Map<List<GroupDto>>(groups);
        
        await cacheService.SetAsync(Constants.AvailableGroupsKey, groupDtos, cancellationToken: cancellationToken);

        return Result.Ok();
    }
}