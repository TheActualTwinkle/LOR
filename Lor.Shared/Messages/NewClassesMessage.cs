using Lor.Shared.Messaging.Models;

namespace Shared.Messaging;

public record NewClassesMessage
{
    public required string GroupName { get; init; }
    
    public required IEnumerable<ClassModel> Classes { get; init; }
}