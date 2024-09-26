using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public struct CreateClassesCommand : IRequest<Result>
{
    // TODO: Должен быть GroupId
    public required string GroupName { get; init; }

    public required Dictionary<string, DateOnly> Classes { get; init; }
}