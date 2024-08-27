using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public struct GetClassQueueQuery : IRequest<Result<List<QueueDto>>>
{
    public required int ClassId { get; init; }
}