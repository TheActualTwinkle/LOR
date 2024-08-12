using FluentValidation;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public class GetQueueQueryValidator : AbstractValidator<GetQueueQuery>
{
    public GetQueueQueryValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.ClassId).NotNull(); //TODO: переделать
    }
}