using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserGroup;

public class GetUserGroupQuery : IRequest<Result<UserDto>>
{
    public long TelegramId { get; init; }
}