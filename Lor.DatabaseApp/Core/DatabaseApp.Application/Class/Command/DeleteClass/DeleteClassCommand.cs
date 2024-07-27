using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClass;

public struct DeleteClassCommand : IRequest<Result>
{
   public List<Domain.Models.Class> OutdatedClassList { get; init; }  
}