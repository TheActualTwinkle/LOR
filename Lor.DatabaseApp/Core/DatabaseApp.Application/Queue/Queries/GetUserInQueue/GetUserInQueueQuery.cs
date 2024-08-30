using DatabaseApp.Application.User;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.IsUserInQueue;

public struct GetUserInQueueQuery : IRequest<Result<UserDto?>>
{
    public required long TelegramId { get; init; }
    public required int ClassId { get; init; }
}