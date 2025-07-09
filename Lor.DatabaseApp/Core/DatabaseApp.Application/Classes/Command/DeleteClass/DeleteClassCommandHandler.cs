using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Classes.Command.DeleteClasses;

public class DeleteClassCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<DeleteClassCommand, Result>
{
    public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        var classRepository = unitOfWork.GetRepository<IClassRepository>();
        
        var @class = await classRepository.GetClassById(request.ClassId, cancellationToken);
            
        if (@class is null) 
            return Result.Fail($"Пара {@class?.Name} не найдена.");
            
        classRepository.Delete(@class);
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        var groupRepository = unitOfWork.GetRepository<IGroupRepository>();
        
        var groups = await groupRepository.GetGroups(cancellationToken);

        if (groups is null) 
            return Result.Fail("Группы не найдены.");

        foreach (var group in groups)
        {
            var classes = await classRepository.GetClassesByGroupId(group.Id, cancellationToken);

            if (classes is null) continue;
            
            await cacheService.SetAsync(Constants.AvailableClassesPrefix + group.Id, classes
                .Adapt<List<ClassDto>>(), cancellationToken: cancellationToken);
        }
        
        return Result.Ok();
    }
}