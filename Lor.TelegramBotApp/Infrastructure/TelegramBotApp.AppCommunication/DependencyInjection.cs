using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotApp.AppCommunication.Consumers;
using TelegramBotApp.AppCommunication.Consumers.Settings;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public static class DependencyInjection
{
    public static IServiceCollection AddCommunicators(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseCommunicationClient, GrpcDatabaseClient>(s =>
        {
            var logger = s.GetRequiredService<ILogger<GrpcDatabaseClient>>();
            
            var url = configuration.GetRequiredSection("profiles:Database-http:applicationUrl").Value ??
                      throw new InvalidOperationException("GrpcDatabaseCommunicationClient url is not set.");
            
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
            x.AddConsumer<UpcomingClassesConsumer>();

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
                
                cfg.ReceiveEndpoint(
                    "dba-new-classes", 
                    e => e.Consumer<NewClassesConsumer>(context));
                
                cfg.ReceiveEndpoint(
                    "dba-upcoming-classes", 
                    e => e.Consumer<UpcomingClassesConsumer>(context));
            });
        });

        services.AddScoped<ConsumerSettings>(_ => 
            new ConsumerSettings(configuration
            .GetRequiredSection("ConsumersSettings")
            .GetValue<TimeSpan>("DefaultCancellationTimeout")));

        return services;
    }
}