using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;

public record DeleteSubscriberCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }
}