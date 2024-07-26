using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public class GetQueueQuery : IRequest<Result>
{
    public long TelegramId { get; init; }
    public int ClassId { get; init; }
}