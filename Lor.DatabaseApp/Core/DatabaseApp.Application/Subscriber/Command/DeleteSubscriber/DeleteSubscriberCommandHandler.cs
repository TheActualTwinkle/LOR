using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.DeleteSubscriber;

public class DeleteSubscriberCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSubscriberCommand, Result>
{
    public async Task<Result> Handle(DeleteSubscriberCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден. Возможно вы не авторизированны?");

        Domain.Models.Subscriber? subscriber = await unitOfWork.SubscriberRepository.GetSubscriberByUserId(user.Id, cancellationToken);
        
        if (subscriber is null) return Result.Fail("Вы отписаны от уведомлений о новых лабораторных работах");

        unitOfWork.SubscriberRepository.Delete(subscriber);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        return Result.Ok();
    }
}