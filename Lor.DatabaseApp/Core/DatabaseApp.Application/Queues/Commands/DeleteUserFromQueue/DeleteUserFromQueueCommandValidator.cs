using FluentValidation;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteUserFromQueueCommandValidator : AbstractValidator<DeleteUserFromQueueCommand>
{
    public DeleteUserFromQueueCommandValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}