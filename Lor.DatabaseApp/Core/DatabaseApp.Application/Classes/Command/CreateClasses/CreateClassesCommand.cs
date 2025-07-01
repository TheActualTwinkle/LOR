using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Classes.Command;

public record CreateClassesCommand : IRequest<Result>
{
    public required string GroupName { get; init; }

    public required Dictionary<string, DateOnly> Classes { get; init; }
}