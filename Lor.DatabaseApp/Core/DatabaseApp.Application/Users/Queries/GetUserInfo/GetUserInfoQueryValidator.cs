using FluentValidation;

namespace DatabaseApp.Application.User.Queries.GetUserInfo;

public class GetUserInfoQueryValidator : AbstractValidator<GetUserInfoQuery>
{
    public GetUserInfoQueryValidator() => 
        RuleFor(x => x.TelegramId).GreaterThan(0);
}