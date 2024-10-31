using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClass;

public class GetClassQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) 
    : IRequestHandler<GetClassQuery, Result<ClassDto>>
{
    public async Task<Result<ClassDto>> Handle(GetClassQuery request, CancellationToken cancellationToken)
    {
        var @class = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);

        return @class is null ? Result.Fail("Такой пары не существует.") : mapper.From(@class).AdaptToType<ClassDto>();
    }
}