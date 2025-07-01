using DatabaseApp.Application.Classes;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.CreateEntry;

public record CreateQueueEntryCommand : IRequest<Result<CreateQueueEntryResponse>>
{
    public required long TelegramId { get; init; }

    public required int ClassId { get; init; }
}

public record CreateQueueEntryResponse
{
    public required bool WasAlreadyEnqueued { get; init; }
    
    public required ClassDto Class { get; init; }
}