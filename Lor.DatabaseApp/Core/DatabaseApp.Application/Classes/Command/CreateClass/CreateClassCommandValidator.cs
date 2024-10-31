using FluentValidation;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public class CreateClassesCommandValidator : AbstractValidator<CreateClassesCommand>
{
    public CreateClassesCommandValidator()
    {
        RuleFor(x => x.GroupId).GreaterThan(0);
        RuleFor(x => x.Classes).NotNull().NotEmpty().Must(HaveValidClasses);
    }

    private bool HaveValidClasses(Dictionary<string, DateOnly> classes)
    {
        if (classes.Count == 0)
        {
            return false;
        }

        foreach ((var className, var date) in classes)
        {
            if (string.IsNullOrEmpty(className.Trim()))
            {
                return false;
            }
            
            if (string.IsNullOrEmpty(date.ToString().Trim()) || date == default)
            {
                return false;
            }
        }

        return true;
    }
}