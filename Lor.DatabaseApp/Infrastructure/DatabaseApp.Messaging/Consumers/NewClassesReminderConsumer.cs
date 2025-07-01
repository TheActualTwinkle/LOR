using DatabaseApp.Domain.Services.ReminderService;
using DatabaseApp.Messaging.Consumers.Settings;
using DatabaseApp.Messaging.Messages;
using MassTransit;

namespace DatabaseApp.Messaging.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesReminderConsumer(
    IClassReminderService classReminderService,
    ConsumerSettings settings)
    : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context) =>
        await classReminderService.ScheduleNotification(
            context.Message.Classes,
            new CancellationTokenSource(settings.DefaultCancellationTimeout).Token);
}