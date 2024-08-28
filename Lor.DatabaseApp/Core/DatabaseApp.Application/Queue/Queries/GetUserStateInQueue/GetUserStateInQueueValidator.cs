using FluentValidation;

namespace DatabaseApp.Application.Queue.Queries.IsUserInQueue;

public class GetUserStateInQueueValidator : AbstractValidator<GetUserStateInQueueQuery>
{
    public GetUserStateInQueueValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}