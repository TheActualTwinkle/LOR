using FluentValidation;

namespace DatabaseApp.Application.QueueEntries.Queries;

public class GetUserInQueueValidator : AbstractValidator<GetUserInQueueQuery>
{
    public GetUserInQueueValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}