using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteOutdatedQueues;

public struct DeleteQueuesForClassesCommand : IRequest<Result>
{
    public IReadOnlyCollection<int> ClassesId { get; init; }
}