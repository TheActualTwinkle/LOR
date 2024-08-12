using FluentValidation;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty();
        RuleFor(x => x.ClassName).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
    }
}