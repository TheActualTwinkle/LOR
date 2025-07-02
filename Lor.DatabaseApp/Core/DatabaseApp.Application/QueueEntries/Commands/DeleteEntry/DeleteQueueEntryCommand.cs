using DatabaseApp.Application.Classes;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteEntry;

public record DeleteQueueEntryCommand : IRequest<Result<DeleteQueueEntryResponse>>
{
    public required int ClassId { get; init; }
    public required long TelegramId { get; init; }
}

public record DeleteQueueEntryResponse
{
    public required bool WasAlreadyDequeued { get; init; }
    
    public required ClassDto Class { get; init; }
}