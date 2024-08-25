using FluentValidation;

namespace DatabaseApp.Application.Queue.Queries.IsUserInQueue;

public class IsUserInQueueQueryValidator : AbstractValidator<IsUserInQueueQuery>
{
    public IsUserInQueueQueryValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}