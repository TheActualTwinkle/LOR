using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command;

public record CreateClassesCommand : IRequest<Result>
{
    public required string GroupName { get; init; }

    public required Dictionary<string, DateOnly> Classes { get; init; }
}