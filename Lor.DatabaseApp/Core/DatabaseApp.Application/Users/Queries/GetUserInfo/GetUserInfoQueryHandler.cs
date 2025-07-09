using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Users.Queries;

public class GetUserInfoQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<GetUserInfoQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var cachedUser = await cacheService.GetAsync<UserDto>(Constants.UserPrefix + request.TelegramId, cancellationToken);

        if (cachedUser is not null) 
            return Result.Ok(cachedUser);
        
        var userRepository = unitOfWork.GetRepository<IUserRepository>();
        
        var user = await userRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) 
            return Result.Fail("Пользователь не найден.");

        var groupRepository = unitOfWork.GetRepository<IGroupRepository>();
        
        var group = await groupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) 
            return Result.Fail("Группа не найдена.");

        var userDto = user.Adapt<UserDto>();
        
        await cacheService.SetAsync(Constants.UserPrefix + request.TelegramId, userDto, cancellationToken: cancellationToken);

        return Result.Ok(userDto);
    }
}