using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public struct DeleteQueueCommand : IRequest<Result>
{
    public List<Domain.Models.Class> OutdatedClaasList { get; init; }
}