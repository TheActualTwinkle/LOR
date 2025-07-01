using DatabaseApp.Domain.Models;
using DatabaseApp.Messaging.Messages;
using Mapster;
using MassTransit;
using MediatR;

namespace DatabaseApp.Application.Classes.Command.Events;

public class ClassesCreatedEventHandler(IBus bus) : INotificationHandler<ClassesCreatedEvent>
{
    public async Task Handle(ClassesCreatedEvent notification, CancellationToken cancellationToken)
    {
        NewClassesMessage newClassesMessage = new()
        {
            GroupName = notification.GroupName,
            Classes = notification.Classes.Adapt<IEnumerable<Class>>()
        };

        await bus.Publish(newClassesMessage, cancellationToken);
    }
}