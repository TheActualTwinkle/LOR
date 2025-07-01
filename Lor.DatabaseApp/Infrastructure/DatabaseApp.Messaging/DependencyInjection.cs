using DatabaseApp.Messaging.Consumers;
using DatabaseApp.Messaging.Consumers.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            
            x.AddConsumer<NewClassesReminderConsumer>();
            x.AddConsumer<NewClassesRemovalConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var configurationSection = configuration.GetRequiredSection("RabbitMqSettings");
                var host = configurationSection["Host"]!;
                var username = configurationSection["Username"]!;
                var password = configurationSection["Password"]!;

                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ReceiveEndpoint(
                    "dba-new-classes-reminder", 
                    e => e.Consumer<NewClassesReminderConsumer>(context));
                
                cfg.ReceiveEndpoint(
                    "dba-new-classes-removal", 
                    e => e.Consumer<NewClassesRemovalConsumer>(context));
            });
        });
        
        var defaultCancellationTimeout = configuration
            .GetRequiredSection("ConsumersSettings")
            .GetValue<TimeSpan>("DefaultCancellationTimeout");

        services.AddScoped<ConsumerSettings>(_ => new ConsumerSettings
        {
            DefaultCancellationTimeout = defaultCancellationTimeout
        });

        return services;
    }
}