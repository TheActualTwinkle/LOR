using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Classes.Queries;

public class GetClassesQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<GetClassesQuery, Result<List<ClassDto>>>
{
    public async Task<Result<List<ClassDto>>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        var cachedClasses = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + request.GroupName, cancellationToken: cancellationToken);

        if (cachedClasses is not null) return Result.Ok(cachedClasses);
            
        cachedClasses = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + request.GroupName, cancellationToken: cancellationToken);
        
        if (cachedClasses is not null) return Result.Ok(cachedClasses);

        var classes = await unitOfWork.ClassRepository.GetClassesByGroupName(request.GroupName, cancellationToken);
        
        if (classes is null) return Result.Fail("Пары не найдены.");
            
        var classesDto = mapper.From(classes).AdaptToType<List<ClassDto>>();
        
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupName, classesDto, cancellationToken: cancellationToken);
            
        return Result.Ok(classesDto);
    }
}