using FluentValidation;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public class CreateClassesCommandValidator : AbstractValidator<CreateClassesCommand>
{
    public CreateClassesCommandValidator()
    {
        RuleFor(x => x.GroupId).GreaterThan(0);
        RuleForEach(x => x.Classes)
            .Where(classes => !string.IsNullOrEmpty(classes.Key.Trim()) 
                              && !string.IsNullOrEmpty(classes.Value.ToString().Trim())).NotNull().NotEmpty();
    }
}