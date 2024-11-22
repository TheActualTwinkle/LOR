using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public record DeleteClassCommand : IRequest<Result>
{
   public required List<int> ClassesId { get; init; }  
}