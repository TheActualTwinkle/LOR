using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.Identity.EmailLinkFactory;
using TelegramBotApp.Identity.Services.AuthService;
using TelegramBotApp.Identity.Services.EmailService;
using TelegramBotApp.Identity.Services.EmailService.EmailContext.ContentProvider;
using TelegramBotApp.Identity.Services.Interfaces;
using TelegramBotApp.Identity.Services.NstuEmailValidationService;
using TelegramBotApp.Identity.Services.NstuGroupService;
using TelegramBotApp.Identity.Services.RegistrationService;
using TelegramBotApp.Identity.Settings.EmailSettings;
using TelegramBotApp.Identity.Validators;

namespace TelegramBotApp.Identity;

/// <summary>
/// Telegram bot identity dependency injection.
/// </summary>
public static class DependencyInjection
{
    private const string EmailSettings = "EmailSettings";

    /// <summary>
    /// Add identity services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSettings = configuration.GetRequiredSection(nameof(EmailSettings)).Get<SmtpEmailSettings>() ??
                            throw new ArgumentNullException(configuration.GetSection(nameof(EmailSettings)).Key);

        services.AddSingleton<IRegistrationService, RegistrationService>();
        services.AddSingleton<IAuthService, NstuAuthService>();
        services.AddSingleton<AbstractValidator<string>, EmailValidator>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IEmailContentProvider, EmailContentProvider>();
        services.AddSingleton<IEmailLinkVerificationFactory, EmailLinkVerificationFactory>();
        services.AddSingleton<INstuEmailValidationService, NstuEmailValidationService>();
        services.AddSingleton<INstuGroupService, NstuGroupService>();
        services.AddHttpClient<INstuGroupService, NstuGroupService>();
        services.AddHttpContextAccessor();

        services
            .AddFluentEmail(emailSettings.EmailSender)
            .AddSmtpSender(
                emailSettings.SmtpHost,
                emailSettings.SmtpPort,
                emailSettings.Login,
                emailSettings.Password)
            .AddRazorRenderer();

        return services;
    }
}