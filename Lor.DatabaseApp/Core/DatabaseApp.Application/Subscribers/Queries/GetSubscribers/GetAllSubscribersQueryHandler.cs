using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.Subscribers.Queries;

public class GetAllSubscribersQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    : IRequestHandler<GetAllSubscribersQuery, Result<List<SubscriberDto>>>
{
    public async Task<Result<List<SubscriberDto>>> Handle(GetAllSubscribersQuery request, CancellationToken cancellationToken)
    {
        var cachedSubscriber = 
            await cacheService.GetAsync<List<SubscriberDto>>(Constants.AllSubscribersKey, cancellationToken);

        if (cachedSubscriber is not null)
            return Result.Ok(cachedSubscriber);

        var subscriberRepository = unitOfWork.GetRepository<ISubscriberRepository>();
        
        var subscribers = await subscriberRepository.GetAllSubscribers(cancellationToken);

        if (subscribers is null) 
            return Result.Fail("Подписчики не найдены");
        
        var subscribersDto = subscribers.Adapt<List<SubscriberDto>>();
            
        await cacheService.SetAsync(Constants.AllSubscribersKey, subscribersDto, cancellationToken: cancellationToken);
        
        return Result.Ok(subscribersDto);
    }
}