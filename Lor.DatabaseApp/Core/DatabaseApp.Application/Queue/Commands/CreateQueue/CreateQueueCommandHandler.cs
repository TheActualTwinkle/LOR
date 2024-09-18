using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public class CreateQueueCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateQueueCommand, Result>
{
    public async Task<Result> Handle(CreateQueueCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден.");

        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не поддерживается.");

        Domain.Models.Class? @class = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);
        
        if (@class is null) return Result.Fail("Пара не найдена.");

        uint queueNum =
             Convert.ToUInt32(await unitOfWork.QueueRepository.GetCurrentQueueNum(request.ClassId));

        bool queueExist =
            await unitOfWork.QueueRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);

        if (queueExist)
        {
            return Result.Fail($"Ваша запись на пару \"{@class.Name} - {@class.Date:dd.MM}\" уже создана.");
        }
            
        Domain.Models.Queue queue = new()
        {
            UserId = user.Id,
            ClassId = request.ClassId,
            QueueNum = queueNum + 1
        };
        
        await unitOfWork.QueueRepository.AddAsync(queue, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        return Result.Ok();
    }
}