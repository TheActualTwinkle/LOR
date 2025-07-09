using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Classes.Queries;

public class GetOutdatedClassesQueryHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetOutdatedClassesQuery, Result<List<int>>>
{
    public async Task<Result<List<int>>> Handle(GetOutdatedClassesQuery request, CancellationToken cancellationToken)
    {
        var classRepository = unitOfWork.GetRepository<IClassRepository>();
        
        var classes = await classRepository.GetOutdatedClassesId(cancellationToken);

        return classes;
    }
        
}