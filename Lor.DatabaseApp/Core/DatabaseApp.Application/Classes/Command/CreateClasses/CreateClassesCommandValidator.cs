using FluentValidation;

namespace DatabaseApp.Application.Class.Command.CreateClasses;

public class CreateClassesCommandValidator : AbstractValidator<CreateClassesCommand>
{
    public CreateClassesCommandValidator()
    {
        RuleFor(x => x.GroupName).NotNull().NotEmpty();
        RuleForEach(x => x.Classes)
            .Where(classes => !string.IsNullOrEmpty(classes.Key.Trim()) &&
                              !string.IsNullOrEmpty(classes.Value.ToString().Trim())).NotNull().NotEmpty();
    }
}