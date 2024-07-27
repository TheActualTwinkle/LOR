using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClasses;

public class GetClassesQuery : IRequest<Result<ClassDto>>
{
    public long TelegramId { get; init; }
}