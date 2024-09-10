namespace DatabaseApp.Application.Dto;

public record UserDto
{
    public required string FullName { get; init; }
    public int GroupId { get; init; }
    public required string GroupName { get; init; }
    public required string Email { get; init; }
    public required bool IsEmailConfirmed { get; init; }
}