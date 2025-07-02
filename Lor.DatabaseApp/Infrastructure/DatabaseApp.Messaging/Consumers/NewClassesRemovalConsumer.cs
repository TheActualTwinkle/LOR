using DatabaseApp.Domain.Services.RemovalService;
using DatabaseApp.Messaging.Consumers.Settings;
using DatabaseApp.Messaging.Messages;
using MassTransit;

namespace DatabaseApp.Messaging.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesRemovalConsumer(
    IClassRemovalService classRemovalService,
    ConsumerSettings settings)
    : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context) =>
        await classRemovalService.ScheduleRemoval(
            context.Message.Classes,
            new CancellationTokenSource(settings.DefaultCancellationTimeout).Token);
}