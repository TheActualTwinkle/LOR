﻿using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Command.CreateUser;

public class CreateUserCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUserCommand, Result>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user = await unitOfWork.UserRepository.CheckUser(request.TelegramId, await request.FullName.FormatFio(), cancellationToken);

        if (user is not null) return Result.Fail("Пользователь c таким именем или id уже существует.");

        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");

        Domain.Models.User newUser = new()
        {
            FullName = await request.FullName.FormatFio(),
            TelegramId = request.TelegramId,
            GroupId = group.Id
        };

        await unitOfWork.UserRepository.AddAsync(newUser, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        return Result.Ok();
    }
}