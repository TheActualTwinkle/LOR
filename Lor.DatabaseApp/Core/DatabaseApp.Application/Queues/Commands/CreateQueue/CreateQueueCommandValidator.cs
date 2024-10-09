using FluentValidation;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public class CreateQueueCommandValidator : AbstractValidator<CreateQueueCommand>
{
    public CreateQueueCommandValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}