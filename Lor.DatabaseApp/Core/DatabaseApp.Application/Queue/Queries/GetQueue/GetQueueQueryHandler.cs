using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public class GetQueueQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetQueueQuery, Result<List<QueueDto>>>
{
    public async Task<Result<List<QueueDto>>> Handle(GetQueueQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null) return Result.Fail("Пользователь не найден.");

        uint queueNum = await unitOfWork.QueueRepository.GetUserQueueNum(user.Id, user.GroupId,
            request.ClassId, cancellationToken);

        List<Domain.Models.Queue>? queueList =
            await unitOfWork.QueueRepository.GetQueueList(queueNum, user.GroupId, request.ClassId,
                cancellationToken);

        if (queueList is null || queueList.Count == 0) return Result.Fail("Очередь не найдена.");
        
        return Result.Ok(mapper.From(queueList).AdaptToType<List<QueueDto>>());
    }
}