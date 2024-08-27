using DatabaseApp.Application.Queue.Queries.GetQueue;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.IsUserInQueue;

public class IsUserInQueueQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<IsUserInQueueQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(IsUserInQueueQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null) return Result.Fail("Пользователь не найден.");
        
        bool isUserInQueue =
            await unitOfWork.QueueRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);
        
        return Result.Ok(isUserInQueue);
    }
}