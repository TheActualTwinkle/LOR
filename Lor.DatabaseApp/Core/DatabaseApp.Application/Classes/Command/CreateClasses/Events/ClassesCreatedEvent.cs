using MediatR;

namespace DatabaseApp.Application.Classes.Command.Events;

public record ClassesCreatedEvent : INotification
{
    public required string GroupName { get; init; }
    
    public required IEnumerable<ClassDto> Classes { get; init; }
}