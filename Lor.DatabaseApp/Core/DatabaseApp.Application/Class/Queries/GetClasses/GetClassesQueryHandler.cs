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
        List<ClassDto>? cachedClasses;
            
        if (request.GroupId is not null)
        {
            cachedClasses = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + request.GroupId, cancellationToken: cancellationToken); //TODO: !!!
        
            if (cachedClasses is not null) return Result.Ok(cachedClasses);
        }
        
        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");
            
        cachedClasses = await cacheService.GetAsync<List<ClassDto>>(Constants.AvailableClassesPrefix + group.Id, cancellationToken: cancellationToken); //TODO: !!!
        
        if (cachedClasses is not null) return Result.Ok(cachedClasses);
        
        List<Domain.Models.Class>? classes = request.GroupId is not null 
            ? await unitOfWork.ClassRepository.GetClassesByGroupId(request.GroupId.Value, cancellationToken) 
            : await unitOfWork.ClassRepository.GetClassesByGroupId(group.Id, cancellationToken);
        
        if (classes is null) return Result.Fail("Пары не найдены.");
            
        List<ClassDto> classDtos = mapper.From(classes).AdaptToType<List<ClassDto>>();
        
        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupId, classDtos, cancellationToken: cancellationToken);
            
        return Result.Ok(classDtos);
    }
}