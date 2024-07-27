using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public class CreateQueueCommand : IRequest<Result>
{
    public long TelegramId { get; init; }

    public int ClassId { get; init; }
}