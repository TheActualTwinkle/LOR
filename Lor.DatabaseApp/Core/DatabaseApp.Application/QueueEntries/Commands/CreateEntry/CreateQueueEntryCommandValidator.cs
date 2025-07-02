using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Commands.CreateEntry;

public class CreateQueueEntryCommandValidator : AbstractValidator<CreateQueueEntryCommand>
{
    public CreateQueueEntryCommandValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}