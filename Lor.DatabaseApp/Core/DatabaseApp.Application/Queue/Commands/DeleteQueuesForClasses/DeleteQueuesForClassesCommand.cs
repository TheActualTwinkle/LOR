using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteOutdatedQueues;

public struct DeleteQueuesForClassesCommand : IRequest<Result>
{
    public required IReadOnlyCollection<int> ClassesId { get; init; }
}