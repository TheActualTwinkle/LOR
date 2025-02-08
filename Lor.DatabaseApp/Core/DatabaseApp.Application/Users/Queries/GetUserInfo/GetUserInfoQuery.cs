using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries;

public record GetUserInfoQuery : IRequest<Result<UserDto>>
{
    public required long TelegramId { get; init; }
}