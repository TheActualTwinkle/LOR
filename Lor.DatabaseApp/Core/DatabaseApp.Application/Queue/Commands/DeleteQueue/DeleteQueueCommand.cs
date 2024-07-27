using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommand : IRequest<Result>
{
    public List<Domain.Models.Class> OutdatedClaasList { get; init; }
}