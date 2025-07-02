using DatabaseApp.Application.Users;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries;

public record GetEnqueuedUsersQuery : IRequest<Result<List<UserDto>>>
{
    public required List<QueueEntryDto> Queue { get; init; }
};