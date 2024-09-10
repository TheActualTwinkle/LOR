using DatabaseApp.Application.Dto;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserInfo;

public class GetUserInfoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUserInfoQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден.");

        Domain.Models.Group? group =
            await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        return group is null
            ? Result.Fail("Группа не найдена.")
            : Result.Ok(mapper.From(user).AdaptToType<UserDto>());
    }
}