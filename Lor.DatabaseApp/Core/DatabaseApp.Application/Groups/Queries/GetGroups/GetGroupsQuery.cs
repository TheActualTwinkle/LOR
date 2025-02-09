using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries;

public record GetGroupsQuery : IRequest<Result<List<GroupDto>>>;