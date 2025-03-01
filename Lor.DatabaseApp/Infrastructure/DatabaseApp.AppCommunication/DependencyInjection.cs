﻿using DatabaseApp.AppCommunication.Consumers;
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
        
        var defaultCancellationTimeout = configuration
            .GetRequiredSection("ConsumersSettings")
            .GetValue<TimeSpan>("DefaultCancellationTimeout");

        services.AddScoped<ConsumerSettings>(_ => new ConsumerSettings
        {
            DefaultCancellationTimeout = defaultCancellationTimeout
        });

        return services;
    }

    public static IServiceCollection AddJobSchedule(this IServiceCollection services, IConfiguration configuration)
    {
        AddHangfire(services, configuration);

        AddClassReminderService(services, configuration);

        AddClassRemovalService(services, configuration);

        return services;
    }

    private static void AddHangfire(IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnectionString = configuration.GetConnectionString("HangfireDb");
        
        services.AddHangfire(c =>
            c.UseDynamicJobs()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(hangfireConnectionString))
                .UseFilter(new AutomaticRetryAttribute { Attempts = 3 }));
        
        services.AddHangfireServer(o =>
        {
            o.Queues = ["dba_queue"];
            o.SchedulePollingInterval = TimeSpan.FromSeconds(10);
        });
    }

    private static void AddClassReminderService(IServiceCollection services, IConfiguration configuration)
    {
        var advanceNoticeTime = configuration
            .GetRequiredSection("ClassReminderServiceSettings")
            .GetValue<TimeSpan>("AdvanceNoticeTime");

        services.AddSingleton(_ => new ClassReminderServiceSettings
        {
            AdvanceNoticeTime = advanceNoticeTime
        });

        // Must be scoped to DI Consumers properly
        services.AddScoped<IClassReminderService, ClassReminderService>();
    }

    private static void AddClassRemovalService(IServiceCollection services, IConfiguration configuration)
    {
        var removalAdvanceTime = configuration
            .GetRequiredSection("ClassRemovalServiceSettings")
            .GetValue<TimeSpan>("RemovalAdvanceTime");

        services.AddSingleton(_ => new ClassRemovalServiceSettings
        {
            RemovalAdvanceTime = removalAdvanceTime
        });
        
        services.AddScoped<IClassRemovalService, ClassRemovalService>();
    }
}