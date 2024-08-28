using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.IsUserInQueue;

public class GetUserStateInQueueHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserStateInQueueQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(GetUserStateInQueueQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null) return Result.Fail("Пользователь не найден.");
        
        bool isUserInQueue =
            await unitOfWork.QueueRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);
        
        return Result.Ok(isUserInQueue); // TODO: вернуть юхера
    }
}