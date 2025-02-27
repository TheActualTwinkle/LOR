using FluentValidation;

namespace DatabaseApp.Application.User.Command.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.TelegramId).GreaterThan(0);
        RuleFor(x => x.FullName).NotNull().NotEmpty();
        RuleFor(x => x.GroupName).NotNull().NotEmpty();
    }
}