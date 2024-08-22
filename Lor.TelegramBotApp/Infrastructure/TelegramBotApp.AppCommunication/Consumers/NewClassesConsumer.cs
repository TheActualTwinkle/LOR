using MassTransit;
using TelegramBotApp.AppCommunication.Consumers.Data;

namespace TelegramBotApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesConsumer : IConsumer<NewClassesMessage>
{
    public Task Consume(ConsumeContext<NewClassesMessage> context)
    {
        return Task.CompletedTask;
    }
}