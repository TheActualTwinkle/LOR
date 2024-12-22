using DatabaseApp.Application.User;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries.IsUserInQueue;

public class GetUserInQueueHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUserInQueueQuery, Result<UserDto?>>
{
    public async Task<Result<UserDto?>> Handle(GetUserInQueueQuery request, CancellationToken cancellationToken)
    {
        var user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null) return Result.Fail("Пользователь не найден.");
        
        var isUserInQueue =
            await unitOfWork.QueueEntryRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);
        
        return isUserInQueue == true ? Result.Ok(mapper.From(user).AdaptToType<UserDto?>()) : Result.Ok<UserDto?>(null);
    }
}