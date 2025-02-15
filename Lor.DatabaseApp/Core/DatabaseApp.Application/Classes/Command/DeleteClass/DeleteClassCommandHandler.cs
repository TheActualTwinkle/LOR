using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Class.Command.DeleteClasses;

public class DeleteClassCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<DeleteClassCommand, Result>
{
    public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        var @class = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);
            
        if (@class is null) 
            return Result.Fail($"Пара {@class?.Name} не найдена.");
            
        unitOfWork.ClassRepository.Delete(@class);
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        var groups = await unitOfWork.GroupRepository.GetGroups(cancellationToken);

        if (groups is null) 
            return Result.Fail("Группы не найдены.");

        foreach (var group in groups)
        {
            var classes = await unitOfWork.ClassRepository.GetClassesByGroupId(group.Id, cancellationToken);

            if (classes is null) continue;
            
            await cacheService.SetAsync(Constants.AvailableClassesPrefix + group.Id, mapper.From(classes)
                .AdaptToType<List<ClassDto>>(), cancellationToken: cancellationToken);
        }
        
        return Result.Ok();
    }
}