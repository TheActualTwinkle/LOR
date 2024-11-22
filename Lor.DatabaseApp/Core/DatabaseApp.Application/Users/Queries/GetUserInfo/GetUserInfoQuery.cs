using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserInfo;

public record GetUserInfoQuery : IRequest<Result<UserDto>>
{
    public required long TelegramId { get; init; }
}