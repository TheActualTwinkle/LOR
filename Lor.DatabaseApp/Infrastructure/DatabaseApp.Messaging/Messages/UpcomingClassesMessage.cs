using DatabaseApp.Domain.Models;

namespace DatabaseApp.Messaging.Messages;

public record UpcomingClassesMessage
{
    public required string ClassName { get; init; }
    public required DateOnly ClassDate { get; init; }
    public required IEnumerable<User> Users { get; init; }
}