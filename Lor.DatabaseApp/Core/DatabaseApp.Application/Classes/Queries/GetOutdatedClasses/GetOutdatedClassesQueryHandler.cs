﻿using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetOutdatedClasses;

public class GetOutdatedClassesQueryHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetOutdatedClassesQuery, Result<List<int>>>
{
    public async Task<Result<List<int>>> Handle(GetOutdatedClassesQuery request, CancellationToken cancellationToken)
    {
        return await unitOfWork.ClassRepository.GetOutdatedClassesId(cancellationToken);
    }
}