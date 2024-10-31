using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Common;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Authorization.Interfaces;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITelegramBotInitializer, TelegramBotInitializer>();
        
        services.AddSingleton<ITelegramBot>(s =>
        {
            var logger = s.GetRequiredService<ILogger<TelegramBot>>();

            var botInitializer = s.GetRequiredService<ITelegramBotInitializer>();
            var databaseCommunicator = s.GetRequiredService<IDatabaseCommunicationClient>();
            var authorizationService = s.GetRequiredService<IAuthorizationService>();
            
            return botInitializer.CreateBot(configuration.GetSection("TelegramSettings:BotToken").Value ??
                                            throw new InvalidOperationException("Bot token is not set."),
                botInitializer.CreateReceiverOptions(), databaseCommunicator, authorizationService, logger);
        });

        return services;
    }
}