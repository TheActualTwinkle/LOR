﻿using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public struct CreateQueueCommand : IRequest<Result>
{
    public long TelegramId { get; init; }

    public int ClassId { get; init; }
}