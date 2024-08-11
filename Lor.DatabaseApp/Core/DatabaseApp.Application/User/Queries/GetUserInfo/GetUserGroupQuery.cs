using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserInfo;

public struct GetUserInfoQuery : IRequest<Result<UserDto>>
{
    public long TelegramId { get; init; }
}