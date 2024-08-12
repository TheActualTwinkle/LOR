using FluentValidation;

namespace DatabaseApp.Application.Class.Queries.GetClasses;

public class GetClassesQueryValidator : AbstractValidator<GetClassesQuery>
{
    public GetClassesQueryValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
    }
}