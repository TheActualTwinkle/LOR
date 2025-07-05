using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging;
using TelegramBotApp.AppCommunication.Consumers.Settings;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesConsumer(
    ITelegramBot bot,
    IDatabaseCommunicationClient communicationClient,
    ILogger<NewClassesConsumer> logger,
    ConsumerSettings settings)
    : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context)
    {
        var result = await communicationClient.GetSubscribers();

        if (result.IsFailed)
        {
            logger.LogError("Failed to get subscribers id: {error}", result.Errors.First());
            return;
        }

        var subscribers = result.Value.Where(x => x.GroupName == context.Message.GroupName).ToList();

        var classesString = string.Join('\n', context.Message.Classes.Select(x => $"{x.Name} - {x.Date:dd.MM}"));

        foreach (var subscriber in subscribers)
        {
            var message = $"Доступны новые лабораторные работы! Используйте /hop для записи:\n{classesString}";

            var cancellationTokenSource = new CancellationTokenSource(settings.DefaultCancellationTimeout);

            await bot.SendMessageAsync(
                subscriber.TelegramId,
                message,
                cancellationToken: cancellationTokenSource.Token);
        }
    }
}