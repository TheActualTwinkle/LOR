using DatabaseApp.Application.Common.Converters;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserGroup;

public class GetUserGroupQueryHandler(IUnitOfWork? unitOfWork, UserDto userDto)
    : IRequestHandler<GetUserGroupQuery, Result>
{
    public async Task<Result> Handle(GetUserGroupQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Для авторизации введите /auth <ФИО>");

        Domain.Models.Group? userGroup = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (userGroup is null) return Result.Fail("Группа не найдена.");

        await userDto.Handle(userGroup.GroupName);

        return Result.Ok();
    }
}