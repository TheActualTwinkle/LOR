using FluentResults;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteQueue;

public record DeleteUserFromQueueCommand : IRequest<Result>
{
    public required int ClassId { get; init; }
    public required long TelegramId { get; init; }
}