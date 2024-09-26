using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public class GetClassQueueQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<GetClassQueueQuery, Result<List<QueueDto>>>
{
    public async Task<Result<List<QueueDto>>> Handle(GetClassQueueQuery request, CancellationToken cancellationToken)
    {
        cancellationToken =new CancellationToken();
        List<QueueDto>? queueCache = await cacheService.GetAsync<List<QueueDto>>(Constants.QueuePrefix + request.ClassId, cancellationToken);
        
        if (queueCache is not null) return Result.Ok(queueCache);
        
        List<Domain.Models.Queue>? queueList =
            await unitOfWork.QueueRepository.GetQueueByClassId(request.ClassId, cancellationToken);
        
        if (queueList is null) return Result.Fail("Очередь не найдена.");

        List<QueueDto> queueDto = mapper.From(queueList).AdaptToType<List<QueueDto>>();
        
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, queueDto, cancellationToken: cancellationToken);

        return Result.Ok(queueDto);
    }
}