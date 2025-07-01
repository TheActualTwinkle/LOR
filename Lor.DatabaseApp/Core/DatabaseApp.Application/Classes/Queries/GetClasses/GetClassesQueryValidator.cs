using FluentValidation;

namespace DatabaseApp.Application.Classes.Queries;

public class GetClassesQueryValidator : AbstractValidator<GetClassesQuery>
{
    public GetClassesQueryValidator() => 
        RuleFor(x => x.GroupName).NotNull().NotEmpty();
}