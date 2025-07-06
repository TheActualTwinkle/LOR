using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Groups.Command.CreateGroup;

public record CreateGroupsCommand : IRequest<Result>
{
    public required List<string> GroupNames { get; init; }
}