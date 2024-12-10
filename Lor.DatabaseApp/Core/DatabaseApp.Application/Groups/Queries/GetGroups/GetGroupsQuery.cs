using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroups;

public record GetGroupsQuery : IRequest<Result<List<GroupDto>>>;