using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Commands.CreateQueue;

public class CreateQueueCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateQueueCommand, Result<Domain.Models.Class>>
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    public async Task<Result<Domain.Models.Class>> Handle(CreateQueueCommand request, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        
        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Для авторизации введите /auth <ФИО>");

        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не поддерживается.");

        Domain.Models.Class? someClass = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);
        
        if (someClass is null) return Result.Fail("Пара не найдена.");

        int queueNum =
            await unitOfWork.QueueRepository.GetCurrentQueueNum(user.GroupId, request.ClassId, cancellationToken);

        bool queueExist =
            await unitOfWork.QueueRepository.CheckQueue(user.Id, user.GroupId, request.ClassId, cancellationToken);

        if (queueExist)
        {
            // TODO: Cache it.
            return Result.Fail($"Запись на пару \"{someClass.ClassName} - {someClass.Date}\" уже создана.");
        }
            
        Domain.Models.Queue queue = new()
        {
            UserId = user.Id,
            ClassId = request.ClassId,
            QueueNum = Convert.ToUInt32(queueNum) + 1
        };
        
        await unitOfWork.QueueRepository.AddAsync(queue, cancellationToken);

        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken), cancellationToken);

        _semaphoreSlim.Release();

        return await Task.FromResult(someClass);
    }
}