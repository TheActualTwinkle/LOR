using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Queries;

public class GetClassQueueQueryValidator : AbstractValidator<GetClassQueueQuery>
{
    public GetClassQueueQueryValidator() => 
        RuleFor(x => x.ClassId).GreaterThan(0);
}