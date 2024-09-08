namespace TelegramBotApp.Identity.Settings.EmailSettings;

/// <summary>
/// Represents an SMTP email settings.
/// </summary>
public class SmtpEmailSettings : ISmtpEmailSettings
{
    /// <summary>
    /// Gets the email sender.
    /// </summary>
    public required string EmailSender { get; init; }

    /// <summary>
    /// Gets the SMTP host.
    /// </summary>
    public required string SmtpHost { get; init; }

    /// <summary>
    /// Gets the SMTP port. Default is 25.
    /// </summary>
    public int SmtpPort { get; init; } = 25;

    /// <summary>
    /// Gets the login.
    /// </summary>
    public required string Login { get; init; }

    /// <summary>
    /// Gets the password.
    /// </summary>
    public required string Password { get; init; }
}