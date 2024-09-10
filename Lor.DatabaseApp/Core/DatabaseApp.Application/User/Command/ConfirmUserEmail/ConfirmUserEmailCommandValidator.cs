using FluentValidation;

namespace DatabaseApp.Application.User.Command.ConfirmUserEmail;

public class ConfirmUserEmailCommandValidator : AbstractValidator<ConfirmUserEmailCommand>
{
    public ConfirmUserEmailCommandValidator() => RuleFor(x => x.TokenIdentifier).NotEmpty();
}