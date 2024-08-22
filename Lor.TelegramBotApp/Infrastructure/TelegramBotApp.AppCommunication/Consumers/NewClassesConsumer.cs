using FluentResults;
using MassTransit;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Consumers.Data;
using TelegramBotApp.AppCommunication.Data;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesConsumer(ITelegramBot bot, IDatabaseCommunicationClient communicationClient) : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context)
    {
        Result<IEnumerable<SubscriberInfo>> result = await communicationClient.GetSubscribers();

        if (result.IsFailed)
        {
            Console.WriteLine($"NewClassesConsumer Error: Failed to get subscribers id: {result.Errors.First()}");
            return;
        }

        List<SubscriberInfo> subscribers = result.Value.Where(x => x.GroupId == context.Message.GroupId).ToList();

        foreach (SubscriberInfo subscriber in subscribers)
        {
            await bot.SendMessageAsync(subscriber.TelegramId, "КЕК", new ReplyKeyboardRemove(), new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI and add message
        }
    }
}