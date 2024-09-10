namespace TelegramBotApp.AppCommunication.Data;

public record UserInfo
{
    public required string FullName { get; init; }
    public required string GroupName { get; init; }
    public bool IsEmailConfirmed { get; init; }
}