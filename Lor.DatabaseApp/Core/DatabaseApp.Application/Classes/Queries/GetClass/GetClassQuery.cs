using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClass;

public record GetClassQuery : IRequest<Result<ClassDto>>
{
    public required string ClassName { get; init; }
    public required DateOnly ClassDate { get; init; }
}