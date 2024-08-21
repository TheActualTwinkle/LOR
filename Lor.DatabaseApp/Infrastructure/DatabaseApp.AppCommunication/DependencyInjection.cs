using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.AppCommunication.Consumers;

namespace DatabaseApp.AppCommunication;

public static class DependencyInjection
{
    // TODO: What is 'bus' ???
    public static IServiceCollection AddBus(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<TestConsumer>();
            
            x.UsingRabbitMq((context,cfg) =>
            {
                cfg.Host("localhost", "/", h => { // TODO: DI
                    h.Username("guest"); // TODO: DI
                    h.Password("guest"); // TODO: DI
                });

                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
}