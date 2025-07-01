using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Classes.Queries;

public record GetClassesQuery : IRequest<Result<List<ClassDto>>>
{
    public required string GroupName { get; init; }
}