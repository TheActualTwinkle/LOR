using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.AppCommunication.Consumers;

namespace DatabaseApp.AppCommunication;

public static class DependencyInjection
{
    public static IServiceCollection AddBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<NewClassesConsumer>();
            
            x.UsingRabbitMq((context,cfg) =>
            {
                IConfigurationSection configurationSection = configuration.GetRequiredSection("RabbitMqSettings");
                string host = configurationSection["Host"]!;
                string virtualHost = configurationSection["VirtualHost"]!;
                string username = configurationSection["Username"]!;
                string password = configurationSection["Password"]!;

                cfg.Host(host, virtualHost, h => {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
}