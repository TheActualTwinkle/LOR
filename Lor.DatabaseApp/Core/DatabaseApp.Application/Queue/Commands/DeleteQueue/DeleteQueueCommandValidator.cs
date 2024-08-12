using FluentValidation;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandValidator : AbstractValidator<DeleteQueueCommand>
{
    public DeleteQueueCommandValidator()
    {
        RuleFor(x => x.OutdatedClassList).NotEmpty().NotNull();
    }
}