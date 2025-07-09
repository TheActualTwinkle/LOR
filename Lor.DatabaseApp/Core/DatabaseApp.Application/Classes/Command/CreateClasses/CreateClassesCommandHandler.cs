using DatabaseApp.Application.Classes.Command.Events;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Classes.Command;

public class CreateClassesCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IPublisher publisher)
    : IRequestHandler<CreateClassesCommand, Result>
{
    public async Task<Result> Handle(CreateClassesCommand request, CancellationToken cancellationToken)
    {
        var groupRepository = unitOfWork.GetRepository<IGroupRepository>();
        
        var group = await groupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);

        if (group is null)
            return Result.Fail("Group not found");

        var classRepository = unitOfWork.GetRepository<IClassRepository>();
        
        var createdClasses = await CreateClasses(
            request.Classes.Select(c => new Class
            {
                Name = c.Key,
                Date = c.Value,
                GroupId = group.Id
            }),
            classRepository,
            cancellationToken);
        
        if (!createdClasses.Any())
            return Result.Ok();

        await publisher.Publish(
            new ClassesCreatedEvent
            {
                GroupName = request.GroupName,
                Classes = createdClasses.Adapt<IEnumerable<ClassDto>>()
            },
            cancellationToken);
        
        var classes = await classRepository.GetClassesByGroupName(request.GroupName, cancellationToken);

        await cacheService.SetAsync(
            Constants.AvailableClassesPrefix + request.GroupName,
            classes.Adapt<IEnumerable<ClassDto>>(),
            cancellationToken: cancellationToken);

        return Result.Ok();
    }

    private async Task<IEnumerable<Class>> CreateClasses(IEnumerable<Class> classes, IClassRepository classRepository, CancellationToken cancellationToken = new())
    {
        var createdClasses = new List<Class>();

        foreach (var @class in classes)
        {
            var classExist = await classRepository.GetClassByNameAndDate(@class.Name, @class.Date, cancellationToken);

            if (classExist is not null)
                continue;

            createdClasses.Add(@class);
            
            await classRepository.AddAsync(@class, cancellationToken);
        }

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        return createdClasses;
    }
}