using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public record CreateQueueCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }

    public required int ClassId { get; init; }
}