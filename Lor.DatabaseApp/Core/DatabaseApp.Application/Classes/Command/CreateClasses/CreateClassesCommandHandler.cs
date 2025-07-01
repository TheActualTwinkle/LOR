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
        var group = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);

        if (group is null)
            return Result.Fail("Group not found");

        var createdClasses = await CreateClasses(
            request.Classes.Select(c => new Class
            {
                Name = c.Key,
                Date = c.Value,
                GroupId = group.Id
            }),
            cancellationToken);

        await publisher.Publish(
            new ClassesCreatedEvent
            {
                GroupName = request.GroupName,
                Classes = createdClasses.Adapt<IEnumerable<ClassDto>>()
            },
            cancellationToken);

        var classes = await unitOfWork.ClassRepository.GetClassesByGroupName(request.GroupName, cancellationToken);

        await cacheService.SetAsync(
            Constants.AvailableClassesPrefix + request.GroupName,
            classes.Adapt<IEnumerable<ClassDto>>(),
            cancellationToken: cancellationToken);

        return Result.Ok();
    }

    private async Task<IEnumerable<Class>> CreateClasses(IEnumerable<Class> classes, CancellationToken cancellationToken = new())
    {
        var createdClasses = new List<Class>();

        foreach (var @class in classes)
        {
            var classExist = await unitOfWork.ClassRepository.GetClassByNameAndDate(@class.Name, @class.Date, cancellationToken);

            if (classExist is not null)
                continue;

            createdClasses.Add(@class);

            await unitOfWork.ClassRepository.AddAsync(@class, cancellationToken);
        }

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        return createdClasses;
    }
}