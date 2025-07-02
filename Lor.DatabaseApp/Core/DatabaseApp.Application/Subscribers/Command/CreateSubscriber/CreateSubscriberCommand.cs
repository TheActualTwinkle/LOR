using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscribers.Command.CreateSubscriber;

public record CreateSubscriberCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }
}