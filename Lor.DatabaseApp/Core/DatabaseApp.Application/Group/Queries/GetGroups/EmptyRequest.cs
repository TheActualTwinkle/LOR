using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroups;

public struct EmptyRequest : IRequest<Result<GroupDto>>
{
}