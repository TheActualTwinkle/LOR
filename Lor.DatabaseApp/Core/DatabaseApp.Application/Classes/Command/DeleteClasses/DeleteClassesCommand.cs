using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClasses;

public record DeleteClassesCommand : IRequest<Result>
{
   public required int ClassId { get; init; }  
}