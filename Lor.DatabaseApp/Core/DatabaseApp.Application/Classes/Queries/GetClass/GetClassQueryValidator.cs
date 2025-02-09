using FluentValidation;

namespace DatabaseApp.Application.Class.Queries;

public class GetClassQueryValidator : AbstractValidator<GetClassQuery>
{
    public GetClassQueryValidator() =>
        RuleFor(x => x.ClassName).NotNull().NotEmpty();
}