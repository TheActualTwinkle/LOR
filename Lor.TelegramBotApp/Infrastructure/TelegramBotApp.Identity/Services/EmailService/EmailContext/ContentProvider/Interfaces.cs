namespace TelegramBotApp.Identity.Services.EmailService.EmailContext.ContentProvider;

/// <summary>
/// Represents a provider of email content.
/// </summary>
public interface IEmailContentProvider
{
    /// <summary>
    /// The email subject.
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// The path to the email template.
    /// </summary>
    string PathToTemplate { get; }

    /// <summary>
    /// Gets the keys of the template.
    /// </summary>
    IEnumerable<string> GetTemplateKeys { get; }
}