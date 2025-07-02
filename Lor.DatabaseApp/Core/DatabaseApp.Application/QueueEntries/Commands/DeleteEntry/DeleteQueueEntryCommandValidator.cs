using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteEntry;

public class DeleteQueueEntryCommandValidator : AbstractValidator<DeleteQueueEntryCommand>
{
    public DeleteQueueEntryCommandValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}