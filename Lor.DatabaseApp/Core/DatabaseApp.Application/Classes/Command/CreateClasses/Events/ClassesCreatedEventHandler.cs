using Lor.Shared.Messaging.Models;
using Mapster;
using MassTransit;
using MediatR;
using Shared.Messaging;

namespace DatabaseApp.Application.Classes.Command.Events;

public class ClassesCreatedEventHandler(IBus bus) : INotificationHandler<ClassesCreatedEvent>
{
    public async Task Handle(ClassesCreatedEvent notification, CancellationToken cancellationToken)
    {
        NewClassesMessage newClassesMessage = new()
        {
            GroupName = notification.GroupName,
            Classes = notification.Classes.Adapt<IEnumerable<ClassModel>>()
        };

        await bus.Publish(newClassesMessage, cancellationToken);
    }
}