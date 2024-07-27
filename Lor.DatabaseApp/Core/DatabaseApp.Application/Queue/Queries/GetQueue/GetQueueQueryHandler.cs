using DatabaseApp.Application.Common.Converters;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public class GetQueueQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetQueueQuery, Result<QueueDto>>
{
    public async Task<Result<QueueDto>> Handle(GetQueueQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null) return Result.Fail("Пользователь не найден.");

        uint queueNum = await unitOfWork.QueueRepository.GetUserQueueNum(request.TelegramId, user.GroupId,
            request.ClassId, cancellationToken);

        List<string>? queueList =
            await unitOfWork.QueueRepository.GetQueueList(queueNum, user.GroupId, request.ClassId,
                cancellationToken);

        if (queueList is null) return Result.Fail("Очередь не найдена.");

        QueueDto queueDto = new QueueDto() { QueueList = queueList };

        return Result.Ok(queueDto);
    }
}