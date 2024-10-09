using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public struct DeleteClassCommand : IRequest<Result>
{
   public required List<int> ClassesId { get; init; }  
}