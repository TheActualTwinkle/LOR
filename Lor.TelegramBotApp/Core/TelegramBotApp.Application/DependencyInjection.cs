using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.AppCommunication.Interfaces;
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
            ITelegramBotInitializer botInitializer = s.GetRequiredService<ITelegramBotInitializer>();
            IDatabaseCommunicationClient databaseCommunicator = s.GetRequiredService<IDatabaseCommunicationClient>();
            IAuthorizationService authorizationService = s.GetRequiredService<IAuthorizationService>();
            
            return botInitializer.CreateBot(configuration.GetSection("TelegramSettings:BotToken").Value ??
                                            throw new InvalidOperationException("Bot token is not set."),
                botInitializer.CreateReceiverOptions(), databaseCommunicator, authorizationService);
        });

        return services;
    }
}