using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.Application.Interfaces;

namespace TelegramBotApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITelegramBotInitializer, TelegramBotInitializer>();
        // services.AddSingleton<IResendMessageService, ResendMessageService>();

        services.AddSingleton<ITelegramBot>(s =>
        {
            var botInitializer = s.GetRequiredService<ITelegramBotInitializer>();
            return botInitializer.CreateBot(configuration.GetSection("TelegramSettings:BotToken").Value ??
                                            throw new InvalidOperationException("Bot token is not set."),
                botInitializer.CreateReceiverOptions());
        });

        return services;
    }
}