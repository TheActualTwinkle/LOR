using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetOutdatedClasses;

public class GetOutdatedClassesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) 
    : IRequestHandler<GetOutdatedClassesQuery, Result<List<ClassDto>>>
{
    public async Task<Result<List<ClassDto>>> Handle(GetOutdatedClassesQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Models.Class>? outdatedClasses = await unitOfWork.ClassRepository.GetOutdatedClasses(cancellationToken);

        if (outdatedClasses is null || outdatedClasses.Count == 0) return Result.Fail("Нет устаревших пар");

        return await Task.FromResult(mapper.Map<List<ClassDto>>(outdatedClasses));
    }
}