using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroups;

public struct GetGroupsQuery : IRequest<Result<List<GroupDto>>>;