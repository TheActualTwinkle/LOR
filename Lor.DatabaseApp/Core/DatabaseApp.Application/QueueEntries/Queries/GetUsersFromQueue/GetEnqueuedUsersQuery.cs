using DatabaseApp.Application.User;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries;

public record GetEnqueuedUsersQuery : IRequest<Result<List<UserDto>>>
{
    public required List<QueueEntryDto> Queue { get; init; }
};