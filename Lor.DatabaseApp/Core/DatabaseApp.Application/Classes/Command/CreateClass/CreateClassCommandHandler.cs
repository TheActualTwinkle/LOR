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
        foreach (KeyValuePair<string, DateOnly> item in request.Classes)
        {
            bool classExist = await unitOfWork.ClassRepository.CheckClass(item.Key, item.Value, cancellationToken);

            if (classExist) continue;
            
            Domain.Models.Class @class = new()
            {
                GroupId = request.GroupId,
                Name = item.Key,
                Date = item.Value
            };
            
            await unitOfWork.ClassRepository.AddAsync(@class, cancellationToken);
        }
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        List<Domain.Models.Class>? classes = await unitOfWork.ClassRepository.GetClassesByGroupId(request.GroupId, cancellationToken);

        List<ClassDto> classDtos = mapper.From(classes).AdaptToType<List<ClassDto>>();

        await cacheService.SetAsync(Constants.AvailableClassesPrefix + request.GroupId, classDtos, cancellationToken: cancellationToken);
        
        return Result.Ok();
    }
}