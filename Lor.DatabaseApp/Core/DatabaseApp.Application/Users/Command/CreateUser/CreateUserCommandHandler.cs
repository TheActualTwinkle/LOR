using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Users.Command.CreateUser;

public class CreateUserCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<CreateUserCommand, Result>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userRepository = unitOfWork.GetRepository<IUserRepository>();
        
        var user = await userRepository.IsUserExists(request.TelegramId, FullNameFormatter.Format(request.FullName), cancellationToken);

        if (user is not null) return Result.Fail("Пользователь c таким именем или id уже существует.");

        var groupRepository = unitOfWork.GetRepository<IGroupRepository>();
        
        var group = await groupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");

        Domain.Models.User newUser = new()
        {
            FullName = FullNameFormatter.Format(request.FullName),
            TelegramId = request.TelegramId,
            GroupId = group.Id
        };
        
        await userRepository.AddAsync(newUser, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        await cacheService.SetAsync(Constants.UserPrefix + request.TelegramId, 
            newUser.Adapt<UserDto>(), cancellationToken: cancellationToken);

        return Result.Ok();
    }
}