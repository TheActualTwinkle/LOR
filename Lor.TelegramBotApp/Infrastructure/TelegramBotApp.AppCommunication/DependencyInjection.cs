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
            ILogger<GrpcDatabaseClient> logger = s.GetRequiredService<ILogger<GrpcDatabaseClient>>();
            string url = configuration.GetRequiredSection("profiles:Database-http:applicationUrl").Value ?? throw new InvalidOperationException("GrpcDatabaseCommunicationClient url is not set.");
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
                IConfigurationSection configurationSection = configuration.GetRequiredSection("RabbitMqSettings");
                string host = configurationSection["Host"]!;
                string username = configurationSection["Username"]!;
                string password = configurationSection["Password"]!;
                
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