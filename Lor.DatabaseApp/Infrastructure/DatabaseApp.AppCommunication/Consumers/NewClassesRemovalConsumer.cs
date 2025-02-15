using DatabaseApp.AppCommunication.Consumers.Settings;
using DatabaseApp.AppCommunication.Messages;
using DatabaseApp.AppCommunication.RemovalService.Interfaces;
using MassTransit;

namespace DatabaseApp.AppCommunication.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesRemovalConsumer(
    IClassRemovalService classRemovalService,
    ConsumerSettings settings)
    : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context)
    {
        await classRemovalService.ScheduleRemoval(
            context.Message.Classes,
            new CancellationTokenSource(settings.DefaultCancellationTimeout).Token);
    }
}