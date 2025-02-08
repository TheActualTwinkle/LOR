using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Queries;

public class GetUsersFromQueueQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUsersFromQueueQuery, Result<List<long>>>
{
    public async Task<Result<List<long>>> Handle(GetUsersFromQueueQuery request, CancellationToken cancellationToken)
    {
        List<long> usersTelegramIds = new();

        foreach (var queueEntry in request.Queue)
        {
            var user = await unitOfWork.UserRepository.GetUserByFullName(queueEntry.FullName, cancellationToken);
            
            if (user is null) return Result.Fail("User not found");
            
            usersTelegramIds.Add(user.TelegramId);
        }
        
        return Result.Ok(usersTelegramIds);
    }
}