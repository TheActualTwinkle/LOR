using FluentValidation;

namespace DatabaseApp.Application.Group.Command.CreateGroup;

public class CreateGroupsCommandValidator : AbstractValidator<CreateGroupsCommand>
{
    public CreateGroupsCommandValidator() => RuleFor(x => x.GroupNames).NotNull().NotEmpty();
}