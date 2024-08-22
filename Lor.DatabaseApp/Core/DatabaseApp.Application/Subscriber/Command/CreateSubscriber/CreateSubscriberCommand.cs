using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.CreateSubscriber;

public class CreateSubscriberCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }
}