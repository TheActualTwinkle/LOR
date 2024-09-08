using FluentResults;
using TelegramBotApp.Identity.Services.EmailService.EmailContext;

namespace TelegramBotApp.Identity.Services.Interfaces;

/// <summary>
/// The email service.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Try to send an email.
    /// </summary>
    /// <param name="request">The email request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the email sending.</returns>
    Task<Result<EmailReply>> SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default);
}