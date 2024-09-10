namespace TelegramBotApp.Identity.Settings.EmailSettings;

/// <summary>
/// Represents an SMTP email settings.
/// </summary>
public interface ISmtpEmailSettings
{
    /// <summary>
    /// Gets the email sender.
    /// </summary>
    string EmailSender { get; }

    /// <summary>
    /// Gets the SMTP host.
    /// </summary>
    string SmtpHost { get; }

    /// <summary>
    /// Gets the SMTP port.
    /// </summary>
    int SmtpPort { get; }

    /// <summary>
    /// Gets the login.
    /// </summary>
    string Login { get; }

    /// <summary>
    /// Gets the password.
    /// </summary>
    string Password { get; }
}