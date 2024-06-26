namespace TelegramBotApp.Authorization;

public readonly struct AuthorizationReply(string fullName, string group)
{
    public string FullName { get; } = fullName;
    public string Group { get; } = group;
}