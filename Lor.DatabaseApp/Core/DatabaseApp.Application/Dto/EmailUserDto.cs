namespace DatabaseApp.Application.Dto;

public record EmailUserDto
{
    public required string FullName { get; init; }
    public required long TelegramId { get; init; }
    public required string GroupName { get; init; }
    public required string Email { get; init; }
}