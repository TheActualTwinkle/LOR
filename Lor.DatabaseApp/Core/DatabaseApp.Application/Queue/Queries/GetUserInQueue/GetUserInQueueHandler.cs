using DatabaseApp.Application.Dto;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetUserInQueue;

public class GetUserInQueueHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUserInQueueQuery, Result<UserDto?>>
{
    public async Task<Result<UserDto?>> Handle(GetUserInQueueQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user =
            await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken); 

        if (user is null) return Result.Fail("Пользователь не найден.");
        
        bool isUserInQueue =
            await unitOfWork.QueueRepository.IsUserInQueue(user.Id, request.ClassId, cancellationToken);
        
        return isUserInQueue == true ? Result.Ok(mapper.From(user).AdaptToType<UserDto?>()) : Result.Ok<UserDto?>(null);
    }
}