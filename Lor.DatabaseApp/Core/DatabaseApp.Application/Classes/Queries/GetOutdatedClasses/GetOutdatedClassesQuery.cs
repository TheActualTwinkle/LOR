using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries;

public record GetOutdatedClassesQuery : IRequest<Result<List<int>>>;