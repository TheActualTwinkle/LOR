using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClasses;

public class GetClassesQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<GetClassesQuery, Result<List<ClassDto>>>
{
    public async Task<Result<List<ClassDto>>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        List<ClassDto>? cachedClasses = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + request.GroupId, cancellationToken: cancellationToken);

        if (cachedClasses is not null) return Result.Ok(cachedClasses);
            
        cachedClasses = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + request.GroupId, cancellationToken: cancellationToken);
        
        if (cachedClasses is not null) return Result.Ok(cachedClasses);

        List<Domain.Models.Class>? classes = await unitOfWork.ClassRepository.GetClassesByGroupId(request.GroupId, cancellationToken);
        
        if (classes is null) return Result.Fail("Пары не найдены.");
            
        List<ClassDto> classDtos = mapper.From(classes).AdaptToType<List<ClassDto>>();
        
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupId, classDtos, cancellationToken: cancellationToken);
            
        return Result.Ok(classDtos);
    }
}