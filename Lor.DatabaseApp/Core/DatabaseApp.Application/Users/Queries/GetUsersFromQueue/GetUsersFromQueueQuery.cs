using DatabaseApp.Application.QueueEntries;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries;

public record GetUsersFromQueueQuery : IRequest<Result<List<long>>>
{
    public required List<QueueEntryDto> Queue { get; init; }
};