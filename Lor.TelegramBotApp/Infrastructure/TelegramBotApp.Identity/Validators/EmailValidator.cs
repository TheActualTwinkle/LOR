using FluentValidation;

namespace TelegramBotApp.Identity.Validators;

public class EmailValidator : AbstractValidator<string>
{
    public EmailValidator() => RuleFor(email => email).EmailAddress().WithMessage("Некорректный email");
}