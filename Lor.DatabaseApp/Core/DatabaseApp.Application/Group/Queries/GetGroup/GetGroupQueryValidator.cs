using System.Data;
using FluentResults;
using FluentValidation;

namespace DatabaseApp.Application.Group.Queries.GetGroup;

public class GetGroupQueryValidator : AbstractValidator<GetGroupQuery>
{
    public GetGroupQueryValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty().NotNull();
    }
}