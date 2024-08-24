using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;

public class DeleteSubscriberCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }
}