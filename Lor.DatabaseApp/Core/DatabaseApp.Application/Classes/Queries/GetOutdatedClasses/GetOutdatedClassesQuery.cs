using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetOutdatedClasses;

public record GetOutdatedClassesQuery : IRequest<Result<List<int>>>;