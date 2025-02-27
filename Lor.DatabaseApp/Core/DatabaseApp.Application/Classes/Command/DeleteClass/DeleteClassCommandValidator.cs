using FluentValidation;

namespace DatabaseApp.Application.Class.Command.DeleteClasses;

public class DeleteClassCommandValidator : AbstractValidator<DeleteClassCommand>
{
    public DeleteClassCommandValidator() => 
        RuleFor(x => x.ClassId).GreaterThan(0);
}