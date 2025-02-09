using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Queries;

public class GetUserInQueueQueryValidator : AbstractValidator<GetUserInQueueQuery>
{
    public GetUserInQueueQueryValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}