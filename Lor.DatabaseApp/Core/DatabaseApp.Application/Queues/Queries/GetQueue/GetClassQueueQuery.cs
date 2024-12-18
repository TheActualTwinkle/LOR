﻿using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public record GetClassQueueQuery : IRequest<Result<List<QueueDto>>>
{
    public required int ClassId { get; init; }
}