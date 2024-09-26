using FluentValidation;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandValidator : AbstractValidator<DeleteQueueCommand>
{
    public DeleteQueueCommandValidator()
    {
        RuleFor(x => x.OutdatedClassList).NotEmpty().NotNull().When(x => x.ClassId == null && !x.ClassId.HasValue && x.TelegramId == null && !x.TelegramId.HasValue);
        RuleFor(x => x.TelegramId).NotEmpty().NotNull().GreaterThan(0).When(x => x.OutdatedClassList is null || x.OutdatedClassList.Count == 0);
        RuleFor(x => x.ClassId).NotEmpty().NotNull().GreaterThan(0).When(x => x.OutdatedClassList is null || x.OutdatedClassList.Count == 0);
    }
}