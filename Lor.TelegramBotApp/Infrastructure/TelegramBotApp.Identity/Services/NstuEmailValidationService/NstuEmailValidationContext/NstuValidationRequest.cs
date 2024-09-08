namespace TelegramBotApp.Identity.Services.NstuEmailValidationService.NstuEmailValidationContext;

public record NstuValidationRequest
{
    public string FullName { get; }
    public long TelegramId { get; }
    public string Email { get; }
    public DateTime? DateOfBirth { get; }

    private NstuValidationRequest(string fullName, long telegramId, string email,
        DateTime? dateOfBirth = null) =>
        (FullName, TelegramId, Email, DateOfBirth) = (fullName, telegramId, email, dateOfBirth);

    public static NstuValidationRequest Create(string fullName, long telegramId, string email,
        DateTime? dateOfBirth = null) => new(fullName, telegramId, email, dateOfBirth);
}