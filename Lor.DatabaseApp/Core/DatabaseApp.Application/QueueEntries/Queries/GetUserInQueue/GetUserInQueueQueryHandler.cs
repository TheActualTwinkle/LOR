using DatabaseApp.Application.User;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries;

public class GetUserInQueueQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<GetUserInQueueQuery, Result<UserDto?>>
{
    public async Task<Result<UserDto?>> Handle(GetUserInQueueQuery request, CancellationToken cancellationToken)
    {
        var user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null)
            return Result.Fail("Пользователь не найден.");
        
        var @class = 
            await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);
        
        if (@class is null)
            return Result.Fail("Такой пары не существует.");
        
        var isUserInQueue =
            await unitOfWork.QueueEntryRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);
        
        return isUserInQueue ?
            Result.Ok(mapper.From(user).AdaptToType<UserDto?>()) :
            Result.Ok<UserDto?>(null);
    }
}