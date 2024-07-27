using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Command.CreateGroup;

public struct CreateGroupCommand : IRequest<Result>
{
    public string GroupName { get; init; }
}