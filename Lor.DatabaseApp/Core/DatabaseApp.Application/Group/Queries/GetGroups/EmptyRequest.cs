using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroups;

public class EmptyRequest : IRequest<Result<GroupDto>>
{
}