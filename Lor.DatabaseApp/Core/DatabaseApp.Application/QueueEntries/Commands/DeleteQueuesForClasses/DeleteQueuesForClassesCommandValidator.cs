using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteOutdatedQueues;

public class DeleteQueuesForClassesCommandValidator : AbstractValidator<DeleteQueuesForClassesCommand>
{
    public DeleteQueuesForClassesCommandValidator() => 
        RuleFor(x => x.ClassId).GreaterThan(0);
}