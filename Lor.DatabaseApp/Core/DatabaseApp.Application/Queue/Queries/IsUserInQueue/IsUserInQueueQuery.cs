using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.IsUserInQueue;

public struct IsUserInQueueQuery : IRequest<Result<bool>>
{
    public required long TelegramId { get; init; }
    public required int ClassId { get; init; }
}