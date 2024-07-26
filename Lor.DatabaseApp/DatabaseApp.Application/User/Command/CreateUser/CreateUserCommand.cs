using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Command.CreateUser;

public class CreateUserCommand : IRequest<Result>
{
    public string UserName { get; init; }

    public string GroupName { get; init; }

    public long TelegramId { get; init; }
}