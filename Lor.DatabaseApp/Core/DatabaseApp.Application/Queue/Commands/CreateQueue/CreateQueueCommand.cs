using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public struct CreateQueueCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }

    public required int ClassId { get; init; }
}