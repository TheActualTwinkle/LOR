namespace DatabaseApp.Application.User;

public record UserDto
{
    public required long Id { get; init; }
    public required string FullName { get; init; }
    public required string GroupName { get; init; }
}