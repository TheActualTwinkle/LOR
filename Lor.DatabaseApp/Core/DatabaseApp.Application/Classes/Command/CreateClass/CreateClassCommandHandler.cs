using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Class.Command.CreateClass;

public class CreateClassesCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<CreateClassesCommand, Result>
{
    public async Task<Result> Handle(CreateClassesCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Classes)
        {
            var classExist = await unitOfWork.ClassRepository.GetClassByNameAndDate(item.Key, item.Value, cancellationToken);

            if (classExist is not null) continue;
            
            Domain.Models.Class @class = new()
            {
                GroupId = request.GroupId,
                Name = item.Key,
                Date = item.Value
            };
            
            await unitOfWork.ClassRepository.AddAsync(@class, cancellationToken);
        }
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        var classes = await unitOfWork.ClassRepository.GetClassesByGroupId(request.GroupId, cancellationToken);

        var classesDto = mapper.From(classes).AdaptToType<List<ClassDto>>();

        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupId, classesDto, cancellationToken: cancellationToken);
        
        return Result.Ok();
    }
}