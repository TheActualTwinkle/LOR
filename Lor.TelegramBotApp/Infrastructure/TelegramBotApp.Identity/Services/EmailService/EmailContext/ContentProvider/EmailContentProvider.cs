using System.Reflection;
using TelegramBotApp.Identity.Dto;

namespace TelegramBotApp.Identity.Services.EmailService.EmailContext.ContentProvider;

/// <summary>
/// Represents a provider of email content.
/// </summary>
public class EmailContentProvider : IEmailContentProvider
{
    /// <summary>
    /// The email subject.
    /// </summary>
    public string Subject => "Подтверждение почтового ящика";

    /// <summary>
    /// The path to the email template.
    /// </summary>
    // only one embedded resource is used
    public string PathToTemplate { get; } = Assembly.GetExecutingAssembly().GetManifestResourceNames().First();

    public IEnumerable<string> GetTemplateKeys
    {
        get
        {
            yield return nameof(UserDto.FullName);
            yield return nameof(EmailRequest.EmailVerificationLink);
        }
    }
}