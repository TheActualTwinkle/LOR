using FluentValidation;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty().NotNull();
        RuleFor(x => x.ClassName).NotEmpty().NotNull();
        RuleFor(x => x.Date).NotEmpty().NotNull();
    }
}