using DatabaseApp.AppCommunication.Consumers.Data;
using DatabaseApp.AppCommunication.ReminderService.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DatabaseApp.AppCommunication.Consumers;

public class NewClassesConsumer(ILogger<NewClassesConsumer> logger, IClassReminderService classReminderService) : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context)
    {
        await classReminderService.ScheduleClassesNotification(context.Message.Classes, new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
    }
}