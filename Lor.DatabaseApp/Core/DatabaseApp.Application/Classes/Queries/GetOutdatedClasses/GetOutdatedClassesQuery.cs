using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Classes.Queries;

public record GetOutdatedClassesQuery : IRequest<Result<List<int>>>;