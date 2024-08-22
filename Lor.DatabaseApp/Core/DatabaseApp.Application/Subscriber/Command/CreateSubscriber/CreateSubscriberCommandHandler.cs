using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Command.CreateSubscriber;

public class CreateSubscriberCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSubscriberCommand, Result>
{
    public async Task<Result> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.User? user = await unitOfWork.UserRepository.GetUserByTelegramId(request.TelegramId, cancellationToken);

        if (user is null) return Result.Fail("Пользователь не найден");

        Domain.Models.Subscriber? subscriber = await unitOfWork.SubscriberRepository.GetSubscriberByTelegramId(request.TelegramId, cancellationToken);
        
        if (subscriber is null) return Result.Fail("Подписчик не найден");


        Domain.Models.Subscriber newSubscriber = new Domain.Models.Subscriber
        {
            TelegramId = request.TelegramId
        };

        await unitOfWork.SubscriberRepository.AddAsync(newSubscriber, cancellationToken);

        await Task.Run(async () => await unitOfWork.SaveDbChangesAsync(cancellationToken));

        return Result.Ok();
    }
}