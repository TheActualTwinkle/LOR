﻿using FluentValidation;

namespace DatabaseApp.Application.Subscribers.Command.DeleteSubscriber;

public class DeleteSubscriberCommandValidator : AbstractValidator<DeleteSubscriberCommand>
{
    public DeleteSubscriberCommandValidator() => 
        RuleFor(x => x.TelegramId).GreaterThan(0);
}