using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries;

// TODO: Useless after 1d463466fb1e228f0b72fb05ff4f6d81eb76c711 commit. Has to be removed!
[Obsolete("Will be removed soon...")]
public record GetClassQuery : IRequest<Result<ClassDto>>
{
    public required string ClassName { get; init; }
    public required DateOnly ClassDate { get; init; }
}