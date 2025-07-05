using DatabaseApp.Domain.Models;
using DatabaseApp.Domain.Services.RemovalService;
using DatabaseApp.Messaging.Consumers.Settings;
using Mapster;
using MassTransit;
using Shared.Messaging;

namespace DatabaseApp.Messaging.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewClassesRemovalConsumer(
    IClassRemovalService classRemovalService,
    ConsumerSettings settings)
    : IConsumer<NewClassesMessage>
{
    public async Task Consume(ConsumeContext<NewClassesMessage> context) =>
        await classRemovalService.ScheduleRemoval(
            context.Message.Classes.Adapt<IEnumerable<Class>>(),
            new CancellationTokenSource(settings.DefaultCancellationTimeout).Token);
}