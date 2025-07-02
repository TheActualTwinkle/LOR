using FluentValidation;

namespace DatabaseApp.Application.Classes.Command.DeleteClasses;

public class DeleteClassCommandValidator : AbstractValidator<DeleteClassCommand>
{
    public DeleteClassCommandValidator() => 
        RuleFor(x => x.ClassId).GreaterThan(0);
}