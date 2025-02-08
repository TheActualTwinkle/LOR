using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries;

public record GetGroupQuery : IRequest<Result<GroupDto>>
{
    public required string GroupName { get; init; }
}