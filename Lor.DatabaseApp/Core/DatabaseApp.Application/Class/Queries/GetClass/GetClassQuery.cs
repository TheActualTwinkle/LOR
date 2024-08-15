using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClass;

public struct GetClassQuery : IRequest<Result<ClassDto>>
{
    public required int ClassId { get; init; }
}