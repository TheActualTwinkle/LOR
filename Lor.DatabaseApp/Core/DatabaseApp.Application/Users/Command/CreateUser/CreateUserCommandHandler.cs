using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Users.Command.CreateUser;

public class CreateUserCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<CreateUserCommand, Result>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userRepository = unitOfWork.GetRepository<IUserRepository>();
        
        var user = await unitOfWork.GetRepository<IUserRepository>().IsUserExists(request.TelegramId, FullNameFormatter.Format(request.FullName), cancellationToken);

        if (user is not null) 
            return Result.Fail("Пользователь c таким именем или id уже существует.");
        
        var group = await unitOfWork.GetRepository<IGroupRepository>().GetGroupByGroupName(request.GroupName, cancellationToken);

        if (group is null) 
            return Result.Fail("Группа не найдена.");

        var newUser = new User()
        {
            FullName = FullNameFormatter.Format(request.FullName),
            TelegramId = request.TelegramId,
            GroupId = group.Id
        };
        
        await userRepository.AddAsync(newUser, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        await cacheService.SetAsync(
            Constants.UserPrefix + request.TelegramId, 
            newUser.Adapt<UserDto>(), 
            cancellationToken: cancellationToken);

        return Result.Ok();
    }
}