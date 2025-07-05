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
        foreach (var item in request.GroupNames)
        {
            var groupName = await unitOfWork.GroupRepository.GetGroupByGroupName(item, cancellationToken);

            if (groupName is not null)
                continue;

            Domain.Models.Group group = new()
            {
                Name = item
            };
                
            await unitOfWork.GroupRepository.AddAsync(group, cancellationToken);
        }
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        var groups = await unitOfWork.GroupRepository.GetGroups(cancellationToken);

        if (groups is null) return Result.Fail("Группы не найдены.");
        
        var groupDtos = mapper.Map<List<GroupDto>>(groups);
        
        await cacheService.SetAsync(Constants.AvailableGroupsKey, groupDtos, cancellationToken: cancellationToken);

        return Result.Ok();
    }
}