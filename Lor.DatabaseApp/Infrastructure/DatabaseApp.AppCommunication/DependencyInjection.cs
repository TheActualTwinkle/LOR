using DatabaseApp.Application.Common;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.AppCommunication;

public static class DependencyInjection
{
    public static IServiceCollection AddBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            
            x.UsingRabbitMq((context,cfg) =>
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
    
    public static IServiceCollection AddCommunication(this IServiceCollection services)
    {
        services.AddScoped<ProjectConfig, ProjectConfig>(provider => new ProjectConfig(TimeSpan.FromSeconds(10)));

        return services;
    }
}
