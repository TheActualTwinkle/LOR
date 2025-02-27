using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries;

public record GetClassesQuery : IRequest<Result<List<ClassDto>>>
{
    public required string GroupName { get; init; }
}