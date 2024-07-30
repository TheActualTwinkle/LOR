using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserGroup;

public struct GetUserGroupQuery : IRequest<Result<UserDto>>
{
    public long TelegramId { get; init; }
}