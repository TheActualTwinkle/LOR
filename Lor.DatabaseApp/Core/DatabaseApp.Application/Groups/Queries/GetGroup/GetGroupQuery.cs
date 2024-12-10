using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroup;

public record GetGroupQuery : IRequest<Result<GroupDto>>
{
    public required string GroupName { get; init; }
}