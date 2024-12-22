using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteQueue;

public class DeleteUserFromQueueCommandValidator : AbstractValidator<DeleteUserFromQueueCommand>
{
    public DeleteUserFromQueueCommandValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}