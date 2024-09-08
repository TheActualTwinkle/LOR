namespace TelegramBotApp.Identity.EmailLinkFactory;

/// <summary>
/// The email link verification factory.
/// </summary>
public interface IEmailLinkVerificationFactory
{
    /// <summary>
    /// Generates the email verification link.
    /// </summary>
    /// <param name="tokenIdentifier">The token identifier.</param>
    /// <returns>The email verification link.</returns>
    public string GenerateEmailVerificationLink(Guid tokenIdentifier);
}