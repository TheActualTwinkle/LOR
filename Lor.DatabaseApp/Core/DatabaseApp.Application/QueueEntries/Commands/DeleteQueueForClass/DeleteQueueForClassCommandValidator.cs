using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;

public class DeleteQueueForClassCommandValidator : AbstractValidator<DeleteQueueForClassCommand>
{
    public DeleteQueueForClassCommandValidator() => 
        RuleFor(x => x.ClassId).GreaterThan(0);
}