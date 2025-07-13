using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Subscribers.Command.DeleteSubscriber;

public class DeleteSubscriberCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<DeleteSubscriberCommand, Result>
{
    public async Task<Result> Handle(DeleteSubscriberCommand request, CancellationToken cancellationToken)
    {
        var userRepository = unitOfWork.GetRepository<IUserRepository>();
        
        var user = await userRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Возможно вы не авторизированны?");

        var subscriberRepository = unitOfWork.GetRepository<ISubscriberRepository>();
        
        var subscriber = await subscriberRepository.GetSubscriberByUserId(user.Id, cancellationToken);
        
        if (subscriber is null) return Result.Fail("Вы отписаны от уведомлений о новых лабораторных работах");

        subscriberRepository.Delete(subscriber);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);
        
        var allSubscribers = await subscriberRepository.GetAllSubscribers(cancellationToken);
        
        await cacheService.SetAsync(Constants.AllSubscribersKey, 
            allSubscribers.Adapt<List<SubscriberDto>>(),
            cancellationToken: cancellationToken);

        return Result.Ok();
    }
}