using FluentValidation;

namespace DatabaseApp.Application.Group.Command.CreateGroup;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator() => RuleFor(x => x.GroupNames).NotEmpty().NotNull();
}