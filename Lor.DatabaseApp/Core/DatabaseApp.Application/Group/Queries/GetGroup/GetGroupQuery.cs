using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroup;

public class GetGroupQuery : IRequest<Result<GroupDto>>
{
    public required string GroupName { get; init; }
}