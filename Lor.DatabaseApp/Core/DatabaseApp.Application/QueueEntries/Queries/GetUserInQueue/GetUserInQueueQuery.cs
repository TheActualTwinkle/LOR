using DatabaseApp.Application.User;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries;

public record GetUserInQueueQuery : IRequest<Result<UserDto?>>
{
    public required long TelegramId { get; init; }
    public required int ClassId { get; init; }
}