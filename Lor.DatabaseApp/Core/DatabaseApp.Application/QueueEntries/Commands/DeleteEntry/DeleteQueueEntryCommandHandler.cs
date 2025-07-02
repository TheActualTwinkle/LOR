using DatabaseApp.Application.Classes;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Commands.DeleteEntry;

public class DeleteQueueEntryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<DeleteQueueEntryCommand, Result<DeleteQueueEntryResponse>>
{
    public async Task<Result<DeleteQueueEntryResponse>> Handle(DeleteQueueEntryCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден.");

        // TODO: Проверка группы должна быть на уровне валидации команды
        var group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не поддерживается.");

        var @class = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);
        
        if (@class is null) return Result.Fail("Пара не найдена.");

        var classQueue = await unitOfWork.QueueEntryRepository.GetQueueByClassId(@class.Id, cancellationToken);
            
        if (classQueue is null) return Result.Fail($"Очередь на пару '{@class.Name}' не найдена");
        
        var userQueueNum = await unitOfWork.QueueEntryRepository.GetUserQueueNum(request.TelegramId, request.ClassId, cancellationToken);
            
        var queueEntry = classQueue.FirstOrDefault(x => x.QueueNum == userQueueNum);

        if (queueEntry is null)
            return Result.Ok(
                new DeleteQueueEntryResponse
                {
                    Class = @class.Adapt<ClassDto>(),
                    WasAlreadyDequeued = true
                });
        
        unitOfWork.QueueEntryRepository.Delete(queueEntry);
        
        var queueAfterDeletedEntry = classQueue.Where(x => x.QueueNum > userQueueNum);

        foreach (var item in queueAfterDeletedEntry)
        {
            item.QueueNum -= 1;
            
            unitOfWork.QueueEntryRepository.Update(item);
        }

        classQueue.Remove(queueEntry);
        
        var newQueue = classQueue.Adapt<List<QueueEntryDto>>();
        
        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, newQueue, cancellationToken: cancellationToken);

        return Result.Ok(
            new DeleteQueueEntryResponse
            {
                Class = @class.Adapt<ClassDto>(),
                WasAlreadyDequeued = false
            });
    }
}