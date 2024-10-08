﻿using FluentValidation;

namespace DatabaseApp.Application.Queue.Commands.DeleteOutdatedQueues;

public class DeleteQueuesForClassesCommandValidator : AbstractValidator<DeleteQueuesForClassesCommand>
{
    public DeleteQueuesForClassesCommandValidator()
    { 
        RuleFor(x => x.ClassesId).NotNull().NotEmpty().Must(x => x.Count > 0);
    }
}