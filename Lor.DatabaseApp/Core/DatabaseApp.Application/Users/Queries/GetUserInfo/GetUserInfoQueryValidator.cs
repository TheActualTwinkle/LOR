using FluentValidation;

namespace DatabaseApp.Application.Users.Queries;

public class GetUserInfoQueryValidator : AbstractValidator<GetUserInfoQuery>
{
    public GetUserInfoQueryValidator() => 
        RuleFor(x => x.TelegramId).GreaterThan(0);
}