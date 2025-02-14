using FluentValidation;

namespace DatabaseApp.Application.Class.Command.DeleteClasses;

public class DeleteClassesCommandValidator : AbstractValidator<DeleteClassesCommand>
{
    public DeleteClassesCommandValidator() => 
        RuleFor(x => x.ClassId).GreaterThan(0);
}