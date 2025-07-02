using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Users.Queries;

public record GetUserInfoQuery : IRequest<Result<UserDto>>
{
    public required long TelegramId { get; init; }
}