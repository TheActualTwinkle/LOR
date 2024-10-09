using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.CreateSubscriber;

public struct CreateSubscriberCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }
}