using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Queries.GetQueue;

public class GetClassQueueQueryValidator : AbstractValidator<GetClassQueueQuery>
{
    public GetClassQueueQueryValidator() => 
        RuleFor(x => x.ClassId).GreaterThan(0);
}