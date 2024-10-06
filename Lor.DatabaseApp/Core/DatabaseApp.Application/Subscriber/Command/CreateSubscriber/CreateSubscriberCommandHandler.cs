using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.CreateSubscriber;

public class CreateSubscriberCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<CreateSubscriberCommand, Result>
{
    public async Task<Result> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Возможно вы не авторизированны?");

        Domain.Models.Subscriber? subscriber = await unitOfWork.SubscriberRepository.GetSubscriberByUserId(user.Id, cancellationToken);
        
        if (subscriber is not null) return Result.Fail("Вы уже подписаны");
        
        Domain.Models.Subscriber newSubscriber = new()
        {
            UserId = user.Id
        };

        await unitOfWork.SubscriberRepository.AddAsync(newSubscriber, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        List<SubscriberDto> cachedSubscriptions = await cacheService.GetAsync<List<SubscriberDto>>(Constants.AllSubscribersKey, cancellationToken: cancellationToken) ?? [];
        
        cachedSubscriptions.Add(new SubscriberDto { TelegramId = request.TelegramId, GroupId = user.GroupId });

        await cacheService.SetAsync(Constants.AllSubscribersKey, cachedSubscriptions, cancellationToken: cancellationToken);
        
        return Result.Ok();
    }
}