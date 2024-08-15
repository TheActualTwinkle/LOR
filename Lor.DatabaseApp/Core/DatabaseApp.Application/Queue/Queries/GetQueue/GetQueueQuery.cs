using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public struct GetQueueQuery : IRequest<Result<List<QueueDto>>>
{
    public required long TelegramId { get; init; }
    public required int ClassId { get; init; }
}