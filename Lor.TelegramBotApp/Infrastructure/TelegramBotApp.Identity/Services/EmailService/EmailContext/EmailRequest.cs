namespace TelegramBotApp.Identity.Services.EmailService.EmailContext;

public record EmailRequest
{
    public string UserFullName { get; }
    public string Email { get; }
    public string EmailVerificationLink { get; }

    private EmailRequest(string userFullName, string email, string emailVerificationLink) =>
        (UserFullName, Email, EmailVerificationLink) = (userFullName, email, emailVerificationLink);

    public static EmailRequest Create(string userFullName, string email, string emailVerificationLink) =>
        new(userFullName, email, emailVerificationLink);
}