using DatabaseApp.Application.Users;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
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
        var userRepository = unitOfWork.GetRepository<IUserRepository>();
        
        var user = await userRepository.GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null)
            return Result.Fail("Пользователь не найден.");

        var classRepository = unitOfWork.GetRepository<IClassRepository>();
        
        var @class = await classRepository.GetClassById(request.ClassId, cancellationToken);
        
        if (@class is null)
            return Result.Fail("Такой пары не существует.");
        
        var queueEntryRepository = unitOfWork.GetRepository<IQueueEntryRepository>();
        
        var isUserInQueue =
            await queueEntryRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);
        
        return isUserInQueue ?
            Result.Ok(user.Adapt<UserDto?>()) :
            Result.Ok<UserDto?>(null);
    }
}