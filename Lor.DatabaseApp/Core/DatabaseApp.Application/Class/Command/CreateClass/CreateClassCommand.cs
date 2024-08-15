using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public struct CreateClassCommand : IRequest<Result>
{
    public required string GroupName { get; init; }

    public required string ClassName { get; init; }

    public required DateOnly Date { get; init; }
}