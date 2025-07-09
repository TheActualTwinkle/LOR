using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Subscribers.Command.CreateSubscriber;

public class CreateSubscriberCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<CreateSubscriberCommand, Result>
{
    public async Task<Result> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        var userRepository = unitOfWork.GetRepository<IUserRepository>();
        
        var user = await userRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Возможно вы не авторизированны?");

        var subscriberRepository = unitOfWork.GetRepository<ISubscriberRepository>();
        
        var subscriber = await subscriberRepository.GetSubscriberByUserId(user.Id, cancellationToken);
        
        if (subscriber is not null) return Result.Fail("Вы уже подписаны");
        
        Domain.Models.Subscriber newSubscriber = new()
        {
            UserId = user.Id
        };

        await subscriberRepository.AddAsync(newSubscriber, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        var allSubscribers = await subscriberRepository.GetAllSubscribers(cancellationToken);
        
        await cacheService.SetAsync(Constants.AllSubscribersKey, 
            mapper.From(allSubscribers).AdaptToType<List<SubscriberDto>>(),
            cancellationToken: cancellationToken);
        
        return Result.Ok();
    }
}