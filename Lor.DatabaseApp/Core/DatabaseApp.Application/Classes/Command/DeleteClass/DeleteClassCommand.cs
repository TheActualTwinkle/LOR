using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClasses;

public record DeleteClassCommand : IRequest<Result>
{
   public required int ClassId { get; init; }  
}