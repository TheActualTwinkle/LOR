using FluentValidation;

namespace DatabaseApp.Application.Subscribers.Command.CreateSubscriber;

public class CreateSubscriberCommandValidator : AbstractValidator<CreateSubscriberCommand>
{
    public CreateSubscriberCommandValidator() => 
        RuleFor(x => x.TelegramId).GreaterThan(0);
}