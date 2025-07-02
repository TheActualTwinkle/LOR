using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Classes.Command.DeleteClasses;

public record DeleteClassCommand : IRequest<Result>
{
   public required int ClassId { get; init; }  
}