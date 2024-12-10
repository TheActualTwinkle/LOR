using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClass;

public record GetClassQuery : IRequest<Result<ClassDto>>
{
    public required int ClassId { get; init; }
}