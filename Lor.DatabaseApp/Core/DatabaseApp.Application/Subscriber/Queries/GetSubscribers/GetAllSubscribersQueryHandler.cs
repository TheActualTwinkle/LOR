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
        List<SubscriberDto>? cachedSubscriber = await cacheService.GetAsync<List<SubscriberDto>>(Constants.AllSubscribersKey, cancellationToken);

        if (cachedSubscriber is not null)
        {
            return Result.Ok(cachedSubscriber);
        }
        
        List<Domain.Models.Subscriber>? subscribers = await unitOfWork.SubscriberRepository.GetAllSubscribers(cancellationToken);

        if (subscribers is null) return Result.Fail("Подписчики не найдены");
        
        List<SubscriberDto> subscriberDtos = mapper.From(subscribers).AdaptToType<List<SubscriberDto>>();
        
            
        await cacheService.SetAsync(Constants.AllSubscribersKey, subscriberDtos, cancellationToken: cancellationToken);
        
        return Result.Ok(subscriberDtos);
    }
        
}