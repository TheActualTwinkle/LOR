using FluentValidation;

namespace DatabaseApp.Application.User.Queries;

public class GetUsersFromQueueQueryValidator : AbstractValidator<GetUsersFromQueueQuery>
{
    public GetUsersFromQueueQueryValidator() => 
        RuleFor(x => x.Queue).NotNull().NotEmpty();
}
