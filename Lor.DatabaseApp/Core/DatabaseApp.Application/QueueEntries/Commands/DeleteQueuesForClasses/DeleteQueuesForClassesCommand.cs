using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;

public record DeleteQueuesForClassesCommand : IRequest<Result>
{
    public required IReadOnlyCollection<int> ClassesId { get; init; }
}