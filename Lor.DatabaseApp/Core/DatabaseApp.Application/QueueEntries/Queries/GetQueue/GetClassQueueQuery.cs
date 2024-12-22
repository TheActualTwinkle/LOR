using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries.GetQueue;

public record GetClassQueueQuery : IRequest<Result<List<QueueEntryDto>>>
{
    public required int ClassId { get; init; }
}