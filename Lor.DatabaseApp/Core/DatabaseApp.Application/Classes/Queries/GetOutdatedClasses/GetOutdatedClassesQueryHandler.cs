using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries;

public class GetOutdatedClassesQueryHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetOutdatedClassesQuery, Result<List<int>>>
{
    public async Task<Result<List<int>>> Handle(GetOutdatedClassesQuery request, CancellationToken cancellationToken) =>
        await unitOfWork.ClassRepository.GetOutdatedClassesId(cancellationToken);
}