namespace TelegramBotApp.Identity.Dto;

public record UserDto
{
    public required string FullName { get; init; }
    public required string GroupName { get; init; }
}