using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Group.Command.CreateGroup;

public class CreateGroupCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<CreateGroupCommand, Result>
{
    public async Task<Result> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.GroupNames)
        {
            Domain.Models.Group? groupName = await unitOfWork.GroupRepository.GetGroupByGroupName(item, cancellationToken);

            if (groupName is not null)
            {
                continue;
            }

            Domain.Models.Group group = new()
            {
                Name = item
            };
                
            await unitOfWork.GroupRepository.AddAsync(group, cancellationToken);
        }
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        List<Domain.Models.Group>? groups = await unitOfWork.GroupRepository.GetGroups(cancellationToken);

        if (groups is null) return Result.Fail("Группы не найдены.");
        
        List<GroupDto> groupDtos = mapper.Map<List<GroupDto>>(groups);
        
        await cacheService.SetAsync(Constants.AvailableGroupsKey, groupDtos, cancellationToken: cancellationToken);

        return Result.Ok();
    }
}