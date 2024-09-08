using System.Reflection;
using FluentEmail.Core;
using FluentResults;
using FluentValidation;
using TelegramBotApp.Identity.Common.Extensions;
using TelegramBotApp.Identity.Services.EmailService.EmailContext;
using TelegramBotApp.Identity.Services.EmailService.EmailContext.ContentProvider;
using TelegramBotApp.Identity.Services.Interfaces;

namespace TelegramBotApp.Identity.Services.EmailService;

/// <summary>
/// The email service. See <see cref="IEmailService"/>.
/// </summary>
/// <param name="fluentEmail">The email sender.</param>
/// <param name="emailValidator">The email validator.</param>
public class EmailService(
    IFluentEmail fluentEmail,
    IEmailContentProvider emailContentProvider,
    AbstractValidator<string> emailValidator) : IEmailService
{
    public async Task<Result<EmailReply>> SendEmailAsync(EmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var emailValidationResult = await emailValidator.ValidateAsync(request.Email, cancellationToken);

        if (!emailValidationResult.IsValid)
        {
            return Result.Fail(new Error(emailValidationResult.Errors.First().ErrorMessage));
        }

        await fluentEmail
            .To(request.Email)
            .Subject(emailContentProvider.Subject)
            .ApplyTemplate(emailContentProvider, [request.UserFullName, request.EmailVerificationLink])
            .SendAsync(cancellationToken);

        return Result.Ok(EmailReply.Create($"Письмо отправлено на {request.Email}"));
    }
}