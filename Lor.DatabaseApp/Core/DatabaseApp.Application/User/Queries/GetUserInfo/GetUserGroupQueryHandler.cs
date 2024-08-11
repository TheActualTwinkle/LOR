using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserInfo;

public class GetUserInfoQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserInfoQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Для авторизации введите /auth <ФИО>");

        Domain.Models.Group? userGroup = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (userGroup is null) return Result.Fail("Группа не найдена.");

        UserDto userDto = new() { FullName = user.FullName, GroupName = userGroup.GroupName };

        return Result.Ok(userDto);
    }
}