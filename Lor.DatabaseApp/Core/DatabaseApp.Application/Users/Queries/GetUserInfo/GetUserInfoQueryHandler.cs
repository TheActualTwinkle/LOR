using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.User.Queries.GetUserInfo;

public class GetUserInfoQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<GetUserInfoQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var cachedUser = await cacheService.GetAsync<UserDto>(Constants.UserPrefix + request.TelegramId, cancellationToken);

        if (cachedUser is not null) return Result.Ok(cachedUser);
        
        var user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден.");

        var group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");

        var userDto = mapper.From(new UserDto { FullName = user.FullName, GroupId = user.GroupId, GroupName = group.Name }).AdaptToType<UserDto>();
        
        await cacheService.SetAsync(Constants.UserPrefix + request.TelegramId, userDto, cancellationToken: cancellationToken);

        return Result.Ok(userDto);
    }
}