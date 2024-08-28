using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetQueue;

public class GetClassQueueQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetClassQueueQuery, Result<List<QueueDto>>>
{
    public async Task<Result<List<QueueDto>>> Handle(GetClassQueueQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Models.Queue>? queueList =
            await unitOfWork.QueueRepository.GetQueueByClassId(request.ClassId, cancellationToken);

        return queueList is null ? Result.Fail("Очередь не найдена.") : Result.Ok(mapper.From(queueList).AdaptToType<List<QueueDto>>());
    }
}