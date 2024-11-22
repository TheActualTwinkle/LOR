using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Command.CreateGroup;

public record CreateGroupsCommand : IRequest<Result>
{
    public required List<string> GroupNames { get; init; }
}