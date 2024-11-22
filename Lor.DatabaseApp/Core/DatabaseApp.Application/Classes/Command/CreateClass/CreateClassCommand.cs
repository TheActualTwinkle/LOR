using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public record CreateClassesCommand : IRequest<Result>
{
    public required int GroupId { get; init; }

    public required Dictionary<string, DateOnly> Classes { get; init; }
}