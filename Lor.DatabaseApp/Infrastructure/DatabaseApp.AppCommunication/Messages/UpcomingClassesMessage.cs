using DatabaseApp.Application.User;

namespace DatabaseApp.AppCommunication.Messages;

public record UpcomingClassesMessage
{
    public required string ClassName { get; init; }
    public required DateOnly ClassDate { get; init; }
    public required IEnumerable<UserDto> Users { get; init; }
}