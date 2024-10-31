using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Queries.GetSubscribers;

public class GetAllSubscribersQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<GetAllSubscribersQuery, Result<List<SubscriberDto>>>
{
    public async Task<Result<List<SubscriberDto>>> Handle(GetAllSubscribersQuery request, CancellationToken cancellationToken)
    {
        var cachedSubscriber = await cacheService.GetAsync<List<SubscriberDto>>(Constants.AllSubscribersKey, cancellationToken);

        if (cachedSubscriber is not null)
            return Result.Ok(cachedSubscriber);

        var subscribers = await unitOfWork.SubscriberRepository.GetAllSubscribers(cancellationToken);

        if (subscribers is null) return Result.Fail("Подписчики не найдены");
        
        var subscribersDto = mapper.From(subscribers).AdaptToType<List<SubscriberDto>>();
            
        await cacheService.SetAsync(Constants.AllSubscribersKey, subscribersDto, cancellationToken: cancellationToken);
        
        return Result.Ok(subscribersDto);
    }
        
}