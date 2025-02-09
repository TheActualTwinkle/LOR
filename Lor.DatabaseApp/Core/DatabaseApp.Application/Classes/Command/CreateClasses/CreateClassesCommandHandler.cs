using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Class.Command;

public class CreateClassesCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<CreateClassesCommand, Result>
{
    public async Task<Result> Handle(CreateClassesCommand request, CancellationToken cancellationToken)
    {
        var group = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);
        
        if (group is null) return Result.Fail("Group not found");
        
        foreach (var item in request.Classes)
        {
            var classExist = await unitOfWork.ClassRepository.GetClassByNameAndDate(item.Key, item.Value, cancellationToken);

            if (classExist is not null) continue;
            
            Domain.Models.Class @class = new()
            {
                GroupId = group.Id,
                Name = item.Key,
                Date = item.Value
            };
            
            await unitOfWork.ClassRepository.AddAsync(@class, cancellationToken);
        }
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        var classes = await unitOfWork.ClassRepository.GetClassesByGroupName(request.GroupName, cancellationToken);

        var classesDto = mapper.From(classes).AdaptToType<List<ClassDto>>();

        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupName, classesDto, cancellationToken: cancellationToken);
        
        return Result.Ok();
    }
}