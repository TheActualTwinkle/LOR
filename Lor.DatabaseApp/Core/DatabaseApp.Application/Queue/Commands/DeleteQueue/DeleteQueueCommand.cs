using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public struct DeleteQueueCommand : IRequest<Result>
{
    public List<int>? OutdatedClassList { get; init; }
    public int? ClassId { get; init; }
    public long? TelegramId { get; init; }
}