using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetOutdatedClasses;

public class GetOutdatedClassesQuery : IRequest<Result<List<int>>>;