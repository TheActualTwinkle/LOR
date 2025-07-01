
using DatabaseApp.Domain.Models;

namespace DatabaseApp.Messaging.Messages;

public record NewClassesMessage
{
    public required string GroupName { get; init; }
    public required IEnumerable<Class> Classes { get; init; }
}