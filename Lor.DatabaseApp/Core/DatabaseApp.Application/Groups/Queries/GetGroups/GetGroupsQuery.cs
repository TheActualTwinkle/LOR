using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Groups.Queries;

public record GetGroupsQuery : IRequest<Result<List<GroupDto>>>;