using DatabaseApp.AppCommunication.Class;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Class.Queries.GetClasses;

public class GetClassesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetClassesQuery, Result<ClassDto>>
{
    public async Task<Result<ClassDto>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Для авторизации введите /auth <ФИО>");

        Domain.Models.Group? group = await unitOfWork.GroupRepository.GetGroupByGroupId(user.GroupId, cancellationToken);

        if (group is null) return Result.Fail("Группа не найдена.");

        List<ClassInfoDto>? classes = await unitOfWork.ClassRepository.GetClassesByGroupId(group.Id, cancellationToken);

        if (classes is null) return Result.Fail("Пары не найдены.");

        ClassDto classDto = new() { ClassList = classes };
        
        return Result.Ok(classDto);
    }
}