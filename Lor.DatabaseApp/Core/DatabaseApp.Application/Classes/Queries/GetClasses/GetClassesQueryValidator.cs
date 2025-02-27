using FluentValidation;

namespace DatabaseApp.Application.Class.Queries;

public class GetClassesQueryValidator : AbstractValidator<GetClassesQuery>
{
    public GetClassesQueryValidator() => 
        RuleFor(x => x.GroupName).NotNull().NotEmpty();
}