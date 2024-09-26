using FluentValidation;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteUserFromQueueCommandValidator : AbstractValidator<DeleteUserFromQueueCommand>
{
    public DeleteUserFromQueueCommandValidator()
    {
        RuleFor(x => x.TelegramId).NotEmpty().NotNull().GreaterThan(0);
        RuleFor(x => x.ClassId).NotEmpty().NotNull().GreaterThan(0);
    }
}