using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotApp.AppCommunication.Consumers;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public static class DependencyInjection
{
    public static IServiceCollection AddCommunicators(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseCommunicationClient, GrpcDatabaseClient>(s =>
        {
            var logger = s.GetRequiredService<ILogger<GrpcDatabaseClient>>();
            var url = configuration.GetRequiredSection("profiles:Database-http:applicationUrl").Value ?? throw new InvalidOperationException("GrpcDatabaseCommunicationClient url is not set.");
            return new GrpcDatabaseClient(url, logger);
        });
        return services;
    }
    
    public static IServiceCollection AddBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumer<NewClassesConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var configurationSection = configuration.GetRequiredSection("RabbitMqSettings");
                var host = configurationSection["Host"]!;
                var username = configurationSection["Username"]!;
                var password = configurationSection["Password"]!;
                
                cfg.Host(host, h => {
                    h.Username(username);
                    h.Password(password);
                });
            
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}