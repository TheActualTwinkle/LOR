using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Queries;

public class GetEnqueuedUsersQueryValidator : AbstractValidator<GetEnqueuedUsersQuery>
{
    public GetEnqueuedUsersQueryValidator() => 
        RuleFor(x => x.Queue).NotNull();
}
