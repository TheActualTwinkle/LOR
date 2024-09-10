using DatabaseApp.Application.Dto;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetUserInQueue;

public struct GetUserInQueueQuery : IRequest<Result<UserDto?>>
{
    public required long TelegramId { get; init; }
    public required int ClassId { get; init; }
}