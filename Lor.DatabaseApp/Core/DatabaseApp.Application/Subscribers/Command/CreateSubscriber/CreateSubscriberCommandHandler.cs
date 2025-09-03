using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Subscribers.Command.CreateSubscriber;

public class CreateSubscriberCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<CreateSubscriberCommand, Result>
{
    public async Task<Result> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.GetRepository<IUserRepository>().GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) 
            return Result.Fail("Пользователь не найден. Возможно вы не авторизированны?");

        var subscriberRepository = unitOfWork.GetRepository<ISubscriberRepository>();
        
        var subscriber = await subscriberRepository.GetSubscriberByUserId(user.Id, cancellationToken);
        
        if (subscriber is not null) 
            return Result.Fail("Вы уже подписаны");
        
        var newSubscriber = new Subscriber()
        {
            UserId = user.Id
        };

        await subscriberRepository.AddAsync(newSubscriber, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        var allSubscribers = await subscriberRepository.GetAllSubscribers(cancellationToken);
        
        await cacheService.SetAsync(
            Constants.AllSubscribersKey, 
            allSubscribers.Adapt<List<SubscriberDto>>(),
            cancellationToken: cancellationToken);
        
        return Result.Ok();
    }
}