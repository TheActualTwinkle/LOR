using FluentResults;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Identity.EmailLinkFactory;
using TelegramBotApp.Identity.Services.EmailService.EmailContext;
using TelegramBotApp.Identity.Services.EmailService.EmailContext.ContentProvider;
using TelegramBotApp.Identity.Services.Interfaces;
using TelegramBotApp.Identity.Services.NstuEmailValidationService.NstuEmailValidationContext;
using TelegramBotApp.Identity.Services.NstuGroupService.NstuGroupContext;
using TelegramBotApp.Identity.Services.RegistrationService.RegistrationContext;

namespace TelegramBotApp.Identity.Services.RegistrationService;

/// <summary>
/// Represents a service for registration. See <see cref="IRegistrationService"/>.
/// </summary>
public class RegistrationService(
    IEmailService emailService,
    INstuEmailValidationService emailValidationService,
    INstuGroupService nstuGroupService,
    IEmailContentProvider emailContentProvider,
    IEmailLinkVerificationFactory emailLinkVerificationFactory)
    : IRegistrationService
{
    public async Task<Result<RegistrationReply>> RegisterAsync(
        RegistrationRequest request,
        IDatabaseCommunicationClient databaseCommunicationClient,
        CancellationToken cancellationToken = default)
    {
        var validationReply = await emailValidationService.ValidateAsync(
            NstuValidationRequest.Create(request.FullName, request.TelegramId, request.Email),
            databaseCommunicationClient, cancellationToken);

        if (validationReply.IsFailed) return Result.Fail(new Error(validationReply.Errors.First().ToString()));

        var groupReply = await nstuGroupService.GetGroupAsync(NstuGroupRequest.Create(request.FullName));

        if (groupReply.IsFailed) return Result.Fail(new Error(groupReply.Errors.First().Message));

        var registrationResult = await databaseCommunicationClient.PreregisterUserAsync(
            groupReply.Value.FormattedFullName, request.TelegramId, request.Email, groupReply.Value.GroupName,
            cancellationToken);

        if (registrationResult.IsFailed) return Result.Fail(new Error(registrationResult.Errors.First().Message));

        var emailVerificationLink =
            emailLinkVerificationFactory.GenerateEmailVerificationLink(tokenIdentifier: registrationResult.Value);

        var emailResult = await emailService.SendEmailAsync(
            EmailRequest.Create(
                request.FullName,
                request.Email,
                emailVerificationLink),
            cancellationToken);

        return emailResult.IsFailed
            ? Result.Fail(new Error(emailResult.Errors.First().Message))
            : Result.Ok(RegistrationReply.Create($"Регистрация прошла успешно! {emailResult.Value.Message}"));
    }
}