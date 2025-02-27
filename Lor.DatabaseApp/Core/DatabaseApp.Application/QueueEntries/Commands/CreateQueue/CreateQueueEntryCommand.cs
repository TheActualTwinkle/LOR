using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.CreateQueue;

public record CreateQueueEntryCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }

    public required int ClassId { get; init; }
}