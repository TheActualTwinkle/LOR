using FluentResults;
using MassTransit;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Consumers.Data;
using TelegramBotApp.AppCommunication.Data;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Domain.Models;

namespace TelegramBotApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesConsumer(ITelegramBot bot, IDatabaseCommunicationClient communicationClient) : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context)
    {
        Result<IEnumerable<SubscriberInfo>> result = await communicationClient.GetSubscribersAsync();

        if (result.IsFailed)
        {
            Console.WriteLine($"NewClassesConsumer Error: Failed to get subscribers id: {result.Errors.First()}");
            return;
        }

        List<SubscriberInfo> subscribers = result.Value.Where(x => x.GroupId == context.Message.GroupId).ToList();

        string classesString = string.Join('\n', context.Message.Classes.Select(x => $"{x.Name} - {x.Date:dd.MM}"));

        foreach (SubscriberInfo subscriber in subscribers)
        {
            var message = $"Доступны новые лабораторные работы! Используйте /hop для записи:\n{classesString}";
            await bot.SendMessageAsync(subscriber.TelegramId, message, new ReplyKeyboardRemove(), new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI and add message
        }
    }
}