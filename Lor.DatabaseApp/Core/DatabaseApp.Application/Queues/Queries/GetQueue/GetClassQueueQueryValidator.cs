using FluentValidation;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public class GetClassQueueQueryValidator : AbstractValidator<GetClassQueueQuery>
{
    public GetClassQueueQueryValidator()
    {
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}