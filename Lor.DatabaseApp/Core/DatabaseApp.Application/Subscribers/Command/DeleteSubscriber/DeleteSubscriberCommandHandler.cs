using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;

public class DeleteSubscriberCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<DeleteSubscriberCommand, Result>
{
    public async Task<Result> Handle(DeleteSubscriberCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Возможно вы не авторизированны?");

        var subscriber = await unitOfWork.SubscriberRepository.GetSubscriberByUserId(user.Id, cancellationToken);
        
        if (subscriber is null) return Result.Fail("Вы отписаны от уведомлений о новых лабораторных работах");

        unitOfWork.SubscriberRepository.Delete(subscriber);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        var cachedSubscriptions = await cacheService.GetAsync<List<SubscriberDto>>(Constants.AllSubscribersKey, cancellationToken: cancellationToken) ?? [];
        
        await cacheService.SetAsync(Constants.AllSubscribersKey, cachedSubscriptions.Where(x => x.TelegramId != request.TelegramId),
            cancellationToken: cancellationToken);

        return Result.Ok();
    }
}