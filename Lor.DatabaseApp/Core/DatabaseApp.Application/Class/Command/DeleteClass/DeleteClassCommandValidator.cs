using FluentValidation;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public class DeleteClassCommandValidator : AbstractValidator<DeleteClassCommand>
{
    public DeleteClassCommandValidator() => RuleFor(x => x.ClassesId).NotNull().NotEmpty().Must(x => x.Count > 0);
}