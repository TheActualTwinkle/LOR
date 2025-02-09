using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries;

public class GetClassQueueQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper)
    : IRequestHandler<GetClassQueueQuery, Result<List<QueueEntryDto>>>
{
    public async Task<Result<List<QueueEntryDto>>> Handle(GetClassQueueQuery request, CancellationToken cancellationToken)
    {
        var queueCache = await cacheService.GetAsync<List<QueueEntryDto>>(Constants.QueuePrefix + request.ClassId, cancellationToken);
        
        if (queueCache is not null) return Result.Ok(queueCache);
        
        var queueList =
            await unitOfWork.QueueEntryRepository.GetQueueByClassId(request.ClassId, cancellationToken);
        
        if (queueList is null) return Result.Fail("Очередь не найдена.");

        var queueDto = mapper.From(queueList).AdaptToType<List<QueueEntryDto>>();
        
        await cacheService.SetAsync(Constants.QueuePrefix + request.ClassId, queueDto, cancellationToken: cancellationToken);

        return Result.Ok(queueDto);
    }
}