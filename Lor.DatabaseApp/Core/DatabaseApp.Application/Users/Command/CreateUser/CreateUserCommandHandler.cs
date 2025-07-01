using DatabaseApp.Application.Common.ExtensionsMethods;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Users.Command.CreateUser;

public class CreateUserCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<CreateUserCommand, Result>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.IsUserExists(request.TelegramId, FullNameFormatter.Format(request.FullName), cancellationToken);

        if (user is not null) return Result.Fail("Пользователь c таким именем или id уже существует.");

        var group = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");

        Domain.Models.User newUser = new()
        {
            FullName = FullNameFormatter.Format(request.FullName),
            TelegramId = request.TelegramId,
            GroupId = group.Id
        };

        await unitOfWork.UserRepository.AddAsync(newUser, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        await cacheService.SetAsync(Constants.UserPrefix + request.TelegramId, 
            mapper.From(newUser).AdaptToType<UserDto>(), cancellationToken: cancellationToken);

        return Result.Ok();
    }
}