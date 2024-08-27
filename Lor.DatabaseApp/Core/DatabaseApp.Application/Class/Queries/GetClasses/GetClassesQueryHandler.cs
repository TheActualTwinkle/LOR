using DatabaseApp.Domain.Repositories;
using FluentResults;
using MapsterMapper;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClasses;

public class GetClassesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetClassesQuery, Result<List<ClassDto>>>
{
    public async Task<Result<List<ClassDto>>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupName(request.GroupName, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");

        List<Domain.Models.Class>? classes = await unitOfWork.ClassRepository.GetClassesByGroupId(group.Id, cancellationToken);

        return classes is null ? Result.Fail("Пары не найдены.") : Result.Ok(mapper.From(classes).AdaptToType<List<ClassDto>>());
    }
}