using FluentValidation;

namespace DatabaseApp.Application.Class.Queries.GetClasses;

public class GetClassesQueryValidator : AbstractValidator<GetClassesQuery>
{
    public GetClassesQueryValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty().NotNull();
        RuleFor(x => x.GroupId).Must(NullOrValidGroupId);
    }

    private bool NullOrValidGroupId(int? userId)
    {
        switch (userId)
        {
            case null:
            case > 0:
                return true;
            default:
                return false;
        }
    }
}