using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Services.ReminderService;
using DatabaseApp.Messaging.Consumers.Settings;
using Mapster;
using MassTransit;
using Shared.Messaging;

namespace DatabaseApp.Messaging.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesReminderConsumer(
    IClassReminderService classReminderService,
    ConsumerSettings settings)
    : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context) =>
        await classReminderService.ScheduleNotification(
            context.Message.Classes.Adapt<IEnumerable<Class>>(),
            new CancellationTokenSource(settings.DefaultCancellationTimeout).Token);
}