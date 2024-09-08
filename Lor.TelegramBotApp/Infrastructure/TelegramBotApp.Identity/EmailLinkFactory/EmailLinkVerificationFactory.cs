using Microsoft.AspNetCore.Http.Extensions;

namespace TelegramBotApp.Identity.EmailLinkFactory;

/// <summary>
/// The email link verification factory. See <see cref="IEmailLinkVerificationFactory"/>.
/// </summary>
public class EmailLinkVerificationFactory
    : IEmailLinkVerificationFactory
{
    // consistent with the route name in the controller
    private const string EmailVerificationRouteName = "email-verification";
    private const string Domain = "http://localhost:31402";

    /// <summary>
    /// Generates the email verification link.
    /// </summary>
    /// <param name="tokenIdentifier">The token identifier.</param>
    /// <returns>The email verification link.</returns>
    public string GenerateEmailVerificationLink(Guid tokenIdentifier)
    {
        return new UriBuilder(Domain)
        {
            Path = EmailVerificationRouteName,
            Query = new QueryBuilder { { "token", tokenIdentifier.ToString() } }.ToString()
        }.ToString();
    }
}