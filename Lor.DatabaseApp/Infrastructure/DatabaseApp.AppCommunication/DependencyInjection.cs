using DatabaseApp.AppCommunication.Consumers;
using DatabaseApp.AppCommunication.Consumers.Settings;
using DatabaseApp.AppCommunication.ReminderService;
using DatabaseApp.AppCommunication.ReminderService.Interfaces;
using DatabaseApp.AppCommunication.ReminderService.Settings;
using DatabaseApp.AppCommunication.RemovalService;
using DatabaseApp.AppCommunication.RemovalService.Interfaces;
using DatabaseApp.AppCommunication.RemovalService.Settings;
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
                    "tba-new-classes-reminder", 
                    e => e.Consumer<NewClassesReminderConsumer>(context));
                
                cfg.ReceiveEndpoint(
                    "tba-new-classes-removal", 
                    e => e.Consumer<NewClassesRemovalConsumer>(context));
            });
        });
        
        services.AddScoped<ConsumerSettings>(_ => new ConsumerSettings(TimeSpan.FromSeconds(10)));

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
        
        services.AddHangfireServer(o =>
        {
            o.Queues = ["dba_queue"];
            o.SchedulePollingInterval = TimeSpan.FromSeconds(10);
        });


        var advanceNoticeTime = configuration
            .GetRequiredSection("ClassReminderServiceSettings")
            .GetValue<TimeSpan>("AdvanceNoticeTime");

        services.AddSingleton(_ => new ClassReminderServiceSettings
        {
            AdvanceNoticeTime = advanceNoticeTime
        });

        // Must be scoped to DI Consumers properly
        services.AddScoped<IClassReminderService, ClassReminderService>();
        
        var removalAdvanceTime = configuration
            .GetRequiredSection("ClassRemovalServiceSettings")
            .GetValue<TimeSpan>("RemovalAdvanceTime");

        services.AddSingleton(_ => new ClassRemovalServiceSettings
        {
            RemovalAdvanceTime = removalAdvanceTime
        });
        
        services.AddScoped<IClassRemovalService, ClassRemovalService>();

        return services;
    }
}