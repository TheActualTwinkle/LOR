using DatabaseApp.Application.Users;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries;

public class GetUserInQueueQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserInQueueQuery, Result<UserDto?>>
{
    public async Task<Result<UserDto?>> Handle(GetUserInQueueQuery request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.GetRepository<IUserRepository>().GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null)
            return Result.Fail("Пользователь не найден.");
        
        var @class = await unitOfWork.GetRepository<IClassRepository>().GetClassById(request.ClassId, cancellationToken);
        
        if (@class is null)
            return Result.Fail("Такой пары не существует.");
        
        var isUserInQueue = await unitOfWork.GetRepository<IQueueEntryRepository>().IsUserInQueue(user.Id, request.ClassId, cancellationToken);
        
        return isUserInQueue ?
            Result.Ok(user.Adapt<UserDto?>()) :
            Result.Ok<UserDto?>(null);
    }
}