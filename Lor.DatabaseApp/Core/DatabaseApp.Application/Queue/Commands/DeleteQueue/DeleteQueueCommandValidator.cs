﻿using FluentValidation;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandValidator : AbstractValidator<DeleteQueueCommand>
{
    public DeleteQueueCommandValidator()
    {
        RuleFor(x => x.OutdatedClassList).NotEmpty().NotNull();
        RuleFor(x => x.TelegramId).NotEmpty().NotNull().When(x => x.OutdatedClassList is null || x.OutdatedClassList.Count == 0);
        RuleFor(x => x.ClassId).NotEmpty().NotNull().When(x => x.OutdatedClassList is null || x.OutdatedClassList.Count == 0);
    }
}