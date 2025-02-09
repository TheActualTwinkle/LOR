using DatabaseApp.AppCommunication.Messages;
using MassTransit;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UpcomingClassesConsumer(ITelegramBot bot) : IConsumer<UpcomingClassesMessage>
{
    public async Task Consume(ConsumeContext<UpcomingClassesMessage> context)
    {
        var classesString = string.Join('\n',  $"{context.Message.ClassName} - {context.Message.ClassDate:dd.MM}");

        foreach (var user in context.Message.Users)
        {
            var message = $"Вы в очереди на : {classesString}\n Не забудьте!";
            
            await bot.SendMessageAsync(
                user.Id,
                message,
                new ReplyKeyboardRemove(), 
                new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI and add message
        }
    }
}
