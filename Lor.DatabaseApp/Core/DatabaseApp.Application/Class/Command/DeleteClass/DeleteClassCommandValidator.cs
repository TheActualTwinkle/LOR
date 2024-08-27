using FluentValidation;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public class DeleteClassCommandValidator : AbstractValidator<DeleteClassCommand>
{
    public DeleteClassCommandValidator()
    {
        RuleFor(x => x.ClassesId).NotEmpty().NotNull();
    }
}