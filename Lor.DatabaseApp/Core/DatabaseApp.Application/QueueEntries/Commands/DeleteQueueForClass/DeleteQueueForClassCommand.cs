using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;

public record DeleteQueueForClassCommand : IRequest<Result>
{
    public required int ClassId { get; init; }
}