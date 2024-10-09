using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetOutdatedClasses;

public struct GetOutdatedClassesQuery : IRequest<Result<List<int>>>;