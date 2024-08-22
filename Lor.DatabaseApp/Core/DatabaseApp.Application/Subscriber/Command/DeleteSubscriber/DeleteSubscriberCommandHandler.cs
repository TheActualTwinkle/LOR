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

        if (user is null) return Result.Fail("Пользователь не найден");

        Domain.Models.Subscriber? subscriber = await unitOfWork.SubscriberRepository.GetSubscriberByTelegramId(request.TelegramId, cancellationToken);
        
        if (subscriber is null) return Result.Fail("Подписчик не найден");

        unitOfWork.SubscriberRepository.Delete(subscriber);

        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken));

        return Result.Ok();
    }
}