using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;

public class DeleteSubscriberCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
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
        
        var allSubscribers = await unitOfWork.SubscriberRepository.GetAllSubscribers(cancellationToken);
        
        await cacheService.SetAsync(Constants.AllSubscribersKey, 
            mapper.From(allSubscribers).AdaptToType<List<SubscriberDto>>(),
            cancellationToken: cancellationToken);

        return Result.Ok();
    }
}