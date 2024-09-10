namespace TelegramBotApp.Identity.Services.AuthService.AuthContext;

public readonly struct AuthReply(string fullName, string group)
{
    public string FullName { get; } = fullName;
    public string Group { get; } = group;
}