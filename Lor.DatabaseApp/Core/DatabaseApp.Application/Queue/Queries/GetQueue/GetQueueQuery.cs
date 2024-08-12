using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public struct GetQueueQuery : IRequest<Result<List<QueueDto>>>
{
    public long TelegramId { get; init; }
    public int ClassId { get; init; }
}