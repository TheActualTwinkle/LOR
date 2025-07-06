using FluentValidation;

namespace DatabaseApp.Application.Groups.Queries;

public class GetGroupQueryValidator : AbstractValidator<GetGroupQuery>
{
    public GetGroupQueryValidator() => 
        RuleFor(x => x.GroupName).NotNull().NotEmpty();
}