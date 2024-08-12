﻿using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserInfo;

public class GetUserInfoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUserInfoQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Для авторизации введите /auth <ФИО>");

        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");

        UserDto userDto = mapper.Map<UserDto>(new UserDto{ FullName = user.FullName, GroupName = group.Name });
        
        return Result.Ok(userDto);
    }
}