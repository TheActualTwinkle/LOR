using Lor.Shared.Messaging.Models;

namespace Shared.Messaging;

public record UpcomingClassesMessage
{
    public required string ClassName { get; init; }

    public required DateOnly ClassDate { get; init; }

    public required IEnumerable<UserModel> Users { get; init; }
}