namespace DatabaseApp.Application.User;

public record UserDto
{
    public required string FullName { get; set; }
    public required int GroupId { get; set; }
    public required string GroupName { get; set; }
}