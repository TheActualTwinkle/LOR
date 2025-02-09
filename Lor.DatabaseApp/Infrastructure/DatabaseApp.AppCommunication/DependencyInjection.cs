using DatabaseApp.AppCommunication.Consumers;
using DatabaseApp.AppCommunication.ReminderService;
using DatabaseApp.AppCommunication.ReminderService.Interfaces;
using DatabaseApp.AppCommunication.ReminderService.Settings;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
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
            
            x.AddConsumer<NewClassesConsumer>();

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

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddReminderService(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnectionString = configuration.GetConnectionString("HangfireDb");
        
        services.AddHangfire(c =>
            c.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(hangfireConnectionString))
                .UseFilter(new AutomaticRetryAttribute { Attempts = 3 }));
        
        services.AddHangfireServer(o => o.SchedulePollingInterval = TimeSpan.FromSeconds(10));

        var advanceNoticeTime = configuration
            .GetRequiredSection("ClassReminderServiceSettings")
            .GetValue<TimeSpan>("AdvanceNoticeTime");

        services.AddSingleton(_ => new ClassReminderServiceSettings
        {
            AdvanceNoticeTime = advanceNoticeTime
        });

        // Must be scoped to DI Consumers properly
        services.AddScoped<IClassReminderService, ClassReminderService>();

        return services;
    }
}