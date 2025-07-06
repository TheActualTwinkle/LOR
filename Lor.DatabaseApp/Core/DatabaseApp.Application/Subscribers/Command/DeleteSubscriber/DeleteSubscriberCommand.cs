using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscribers.Command.DeleteSubscriber;

public record DeleteSubscriberCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }
}