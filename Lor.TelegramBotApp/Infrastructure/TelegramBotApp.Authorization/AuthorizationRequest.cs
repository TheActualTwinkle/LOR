namespace TelegramBotApp.Authorization;

public readonly struct AuthorizationRequest(string fullName, DateTime? dateOfBirth = default)
{
    public string FullName { get; } = fullName;
    public DateTime? DateOfBirth { get; } = dateOfBirth;
}