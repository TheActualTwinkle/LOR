using FluentValidation;

namespace DatabaseApp.Application.Group.Queries;

public class GetGroupQueryValidator : AbstractValidator<GetGroupQuery>
{
    public GetGroupQueryValidator() => 
        RuleFor(x => x.GroupName).NotNull().NotEmpty();
}