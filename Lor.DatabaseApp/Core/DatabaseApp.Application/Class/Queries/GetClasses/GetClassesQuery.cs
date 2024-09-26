using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClasses;

public struct GetClassesQuery : IRequest<Result<List<ClassDto>>>
{
    public int GroupId { get; init; }
}