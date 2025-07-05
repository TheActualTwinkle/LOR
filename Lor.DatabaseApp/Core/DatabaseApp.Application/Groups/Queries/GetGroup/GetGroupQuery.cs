using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Groups.Queries;

public record GetGroupQuery : IRequest<Result<GroupDto>>
{
    public required string GroupName { get; init; }
}