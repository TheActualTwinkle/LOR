﻿using FluentValidation;

namespace DatabaseApp.Application.Class.Queries.GetClass;

public class GetClassQueryValidator : AbstractValidator<GetClassQuery>
{
    public GetClassQueryValidator() => RuleFor(x => x.ClassId).GreaterThan(0);
    
}