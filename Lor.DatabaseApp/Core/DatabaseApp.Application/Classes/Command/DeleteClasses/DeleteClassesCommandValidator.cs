using FluentValidation;

namespace DatabaseApp.Application.Class.Command.DeleteClasses;

public class DeleteClassesCommandValidator : AbstractValidator<DeleteClassesCommand>
{
    public DeleteClassesCommandValidator() => 
        RuleFor(x => x.ClassesId).NotNull().NotEmpty();
}