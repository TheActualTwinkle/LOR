using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public class CreateQueueCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateQueueCommand, Result>
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    public async Task<Result> Handle(CreateQueueCommand request, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);

        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Для авторизации введите /auth <ФИО>");

        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");

        int queueNum =
            await unitOfWork.QueueRepository.GetCurrentQueueNum(user.GroupId, request.ClassId, cancellationToken);

        bool queueExist =
            await unitOfWork.QueueRepository.CheckQueue(user.Id, user.GroupId, request.ClassId, cancellationToken);

        if (queueExist) return Result.Fail("Запись уже создана.");
            
        Domain.Models.Queue queue = new()
        {
            UserId = user.Id,
            ClassId = request.ClassId,
            QueueNum = Convert.ToUInt32(queueNum) + 1
        };
        
        await unitOfWork.QueueRepository.AddAsync(queue, cancellationToken);

        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken), cancellationToken);

        _semaphoreSlim.Release();

        return Result.Ok();
    }
}