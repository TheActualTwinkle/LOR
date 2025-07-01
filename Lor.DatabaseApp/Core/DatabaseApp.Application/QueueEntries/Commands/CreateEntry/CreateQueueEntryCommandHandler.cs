using DatabaseApp.Application.Classes;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.CreateEntry;

public class CreateQueueEntryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<CreateQueueEntryCommand, Result<CreateQueueEntryResponse>>
{
    public async Task<Result<CreateQueueEntryResponse>> Handle(CreateQueueEntryCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден.");

        // TODO: Проверка группы должна быть на уровне валидации команды
        var group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не поддерживается.");

        var @class = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);
        
        if (@class is null) return Result.Fail("Пара не найдена.");

        var isUserAlreadyEnqueued =
            await unitOfWork.QueueEntryRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);

        if (isUserAlreadyEnqueued)
            return Result.Ok(
                new CreateQueueEntryResponse
                {
                    Class = @class.Adapt<ClassDto>(),
                    WasAlreadyEnqueued = true
                });

        var queueNum =
            Convert.ToUInt32(await unitOfWork.QueueEntryRepository.GetCurrentQueueNum(request.ClassId));
        
        QueueEntry queueEntry = new()
        {
            UserId = user.Id,
            ClassId = request.ClassId,
            QueueNum = queueNum + 1
        };
        
        await unitOfWork.QueueEntryRepository.AddAsync(queueEntry, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        await cacheService.SetAsync(
            Constants.QueuePrefix + request.ClassId,
            (await unitOfWork.QueueEntryRepository.GetQueueByClassId(request.ClassId, cancellationToken)).Adapt<List<QueueEntryDto>>(),
            cancellationToken: cancellationToken);

        return Result.Ok(
            new CreateQueueEntryResponse
            {
                Class = @class.Adapt<ClassDto>(),
                WasAlreadyEnqueued = false
            });
    }
}