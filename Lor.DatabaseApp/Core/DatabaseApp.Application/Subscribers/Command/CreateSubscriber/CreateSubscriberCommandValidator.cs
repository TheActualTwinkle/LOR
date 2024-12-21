using FluentValidation;

namespace DatabaseApp.Application.Subscriber.Command.CreateSubscriber;

public class CreateSubscriberCommandValidator : AbstractValidator<CreateSubscriberCommand>
{
    public CreateSubscriberCommandValidator() => RuleFor(x => x.TelegramId).GreaterThan(0);
}