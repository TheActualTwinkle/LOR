﻿using DatabaseApp.AppCommunication.Messages;
using MassTransit;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Consumers.Settings;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UpcomingClassesConsumer(ITelegramBot bot, ConsumerSettings settings) : IConsumer<UpcomingClassesMessage>
{
    public async Task Consume(ConsumeContext<UpcomingClassesMessage> context)
    {
        var classesString = string.Join('\n',  $"{context.Message.ClassName} - {context.Message.ClassDate:dd.MM}");

        foreach (var user in context.Message.Users)
        {
            var message = $"Вы в очереди на : {classesString}\nНе забудьте!";
            
            await bot.SendMessageAsync(
                user.TelegramId,
                message,
                new ReplyKeyboardRemove(), 
                new CancellationTokenSource(settings.DefaultCancellationTimeout).Token);
        }
    }
}
