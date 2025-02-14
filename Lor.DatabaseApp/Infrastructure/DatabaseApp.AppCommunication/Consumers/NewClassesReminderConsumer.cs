using DatabaseApp.AppCommunication.Consumers.Settings;
using DatabaseApp.AppCommunication.Messages;
using DatabaseApp.AppCommunication.ReminderService.Interfaces;
using DatabaseApp.AppCommunication.RemovalService.Interfaces;
using MassTransit;

namespace DatabaseApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesReminderConsumer(
    IClassReminderService classReminderService,
    ConsumerSettings settings)
    : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context)
    {
        await classReminderService.ScheduleNotification(
            context.Message.Classes,
            new CancellationTokenSource(settings.DefaultCancellationTimeout).Token);
    }
}