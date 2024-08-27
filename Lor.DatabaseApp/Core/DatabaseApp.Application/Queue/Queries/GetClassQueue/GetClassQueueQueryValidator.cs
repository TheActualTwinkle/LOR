using FluentValidation;

namespace DatabaseApp.Application.Queue.Queries.GetClassQueue;

public class GetClassQueueQueryValidator : AbstractValidator<GetClassQueueQuery>
{
    public GetClassQueueQueryValidator()
    {
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}