using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries;

public record GetClassQuery : IRequest<Result<ClassDto>>
{
    public required string ClassName { get; init; }
    public required DateOnly ClassDate { get; init; }
}