using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public class CreateQueueCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<CreateQueueCommand, Result>
{
    public async Task<Result> Handle(CreateQueueCommand request, CancellationToken cancellationToken)
    {
        var user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден.");

        var group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не поддерживается.");

        var @class = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);
        
        if (@class is null) return Result.Fail("Пара не найдена.");

        var queueNum =
             Convert.ToUInt32(await unitOfWork.QueueRepository.GetCurrentQueueNum(request.ClassId));

        var queueExist =
            await unitOfWork.QueueRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);

        if (queueExist)
            return Result.Fail($"Ваша запись на пару \"{@class.Name} - {@class.Date:dd.MM}\" уже создана.");

        Domain.Models.Queue queue = new()
        {
            UserId = user.Id,
            ClassId = request.ClassId,
            QueueNum = queueNum + 1
        };
        
        await unitOfWork.QueueRepository.AddAsync(queue, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, 
            mapper.From(await unitOfWork.QueueRepository.GetQueueByClassId(request.ClassId, cancellationToken))
                .AdaptToType<List<QueueDto>>(), cancellationToken: cancellationToken);

        return Result.Ok();
    }
}