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
        Domain.Models.Class? someClass = await unitOfWork.ClassRepository.GetClassById(request.ClassId, cancellationToken);

        if (someClass is null) return Result.Fail("Такой пары не существует.");

        return await Task.FromResult(mapper.From(someClass).AdaptToType<ClassDto>());
    }
}