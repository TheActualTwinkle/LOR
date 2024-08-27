using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Queue.Queries.GetClassQueue;

public class GetClassQueueQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetClassQueueQuery, Result<List<QueueDto>>>
{
    public async Task<Result<List<QueueDto>>> Handle(GetClassQueueQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.Class? someClass = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);

        if (someClass is null) return Result.Fail("Пара не найдена.");

        List<Domain.Models.Queue>? queueList = await unitOfWork.QueueRepository.GetQueueList(someClass.GroupId, request.ClassId, cancellationToken);

        return queueList is null ? Result.Fail("Очередь не найдена.") : Result.Ok(mapper.From(queueList).AdaptToType<List<QueueDto>>());
    }
}