using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public class CreateClassCommand : IRequest<Result>
{
    public string GroupName { get; init; }

    public string ClassName { get; init; }

    public DateOnly Date { get; init; }
}