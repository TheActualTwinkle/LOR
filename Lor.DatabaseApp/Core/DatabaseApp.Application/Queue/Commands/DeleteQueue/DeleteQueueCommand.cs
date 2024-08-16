using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public struct DeleteQueueCommand : IRequest<Result>
{
    public required List<int> OutdatedClassList { get; init; }
}