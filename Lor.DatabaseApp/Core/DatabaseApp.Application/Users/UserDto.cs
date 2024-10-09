namespace DatabaseApp.Application.User;

public record UserDto
{
    public required string FullName { get; init; }
    public required int GroupId { get; init; }
    public required string GroupName { get; init; }
}