using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Domain.Models;
using TelegramBotApp.Identity.Services.Interfaces;

namespace TelegramBotApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITelegramBotInitializer, TelegramBotInitializer>();
        
        services.AddSingleton<ITelegramBot>(s =>
        {
            ITelegramBotInitializer botInitializer = s.GetRequiredService<ITelegramBotInitializer>();
            IDatabaseCommunicationClient databaseCommunicator = s.GetRequiredService<IDatabaseCommunicationClient>();
            IRegistrationService registrationService = s.GetRequiredService<IRegistrationService>();
            IAuthService authService = s.GetRequiredService<IAuthService>();

            return botInitializer.CreateBot(configuration.GetSection("TelegramSettings:BotToken").Value ??
                                            throw new InvalidOperationException("Bot token is not set."),
                botInitializer.CreateReceiverOptions(), databaseCommunicator, registrationService, authService);
        });

        return services;
    }
}