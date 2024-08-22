using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Queries.GetSubscribers;

public class GetAllSubscribersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetAllSubscribersQuery, Result<List<SubscriberDto>>>
{
    public async Task<Result<List<SubscriberDto>>> Handle(GetAllSubscribersQuery request, CancellationToken cancellationToken) =>
        Result.Ok(mapper.From(await unitOfWork.SubscriberRepository.GetAllSubscribers(cancellationToken)).AdaptToType<List<SubscriberDto>>());
}