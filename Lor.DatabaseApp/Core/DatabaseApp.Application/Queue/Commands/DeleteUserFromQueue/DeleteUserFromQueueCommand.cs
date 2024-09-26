using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public struct DeleteUserFromQueueCommand : IRequest<Result>
{
    public int ClassId { get; init; }
    public long TelegramId { get; init; }
}