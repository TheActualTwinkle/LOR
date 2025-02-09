using DatabaseApp.AppCommunication.Messages;
using DatabaseApp.AppCommunication.ReminderService.Interfaces;
using MassTransit;

namespace DatabaseApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesConsumer(
    IClassReminderService classReminderService)
    : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context) =>
        await classReminderService.ScheduleNotification(
            context.Message.Classes,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token); // TODO: DI cancellation token
}