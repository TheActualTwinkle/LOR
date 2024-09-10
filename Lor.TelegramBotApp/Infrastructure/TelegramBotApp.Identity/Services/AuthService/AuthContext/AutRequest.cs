namespace TelegramBotApp.Identity.Services.AuthService.AuthContext;

public readonly struct AuthRequest(string fullName, DateTime? dateOfBirth = default)
{
    public string FullName { get; } = fullName;
    public DateTime? DateOfBirth { get; } = dateOfBirth;
}