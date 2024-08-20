using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroupInfo;

public class GetGroupInfoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetGroupInfoQuery, Result<GroupDto>>
{
    public async Task<Result<GroupDto>> Handle(GetGroupInfoQuery request, CancellationToken cancellationToken)
    {
        GroupDto? group;
        if (request.GroupId is not null)
        {
             group =  mapper.From(await unitOfWork.GroupRepository.GetGroupByGroupId(request.GroupId.Value, cancellationToken)).AdaptToType<GroupDto>();

            return group is null ? Result.Fail("Группа не найдена.") : group;
        }
        else if (request.GroupName is not null)
        {
            group = mapper.From(await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken)).AdaptToType<GroupDto>();
        
            return group is null ? Result.Fail("Группа не найдена.") : group;  //TODO: передать
        }

        Domain.Models.User? user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId.Value, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден.");

        group = mapper.From(await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken)).AdaptToType<GroupDto>();
        
        return group is null ? Result.Fail("Группа не найдена.") : group;

    }
}