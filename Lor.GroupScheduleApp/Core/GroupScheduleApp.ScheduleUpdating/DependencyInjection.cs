using GroupScheduleApp.ScheduleUpdating.Settings;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroupScheduleApp.ScheduleUpdating;

public static class DependencyInjection
{
    public static IServiceCollection AddSenderService(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnectionString = configuration.GetConnectionString("HangfireDb");

        services.AddHangfire(c =>
            c.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(hangfireConnectionString))
                .UseFilter(new AutomaticRetryAttribute { Attempts = 3 }));

        services.AddHangfireServer(o => o.SchedulePollingInterval = TimeSpan.FromSeconds(10));

        var intervalString = configuration.GetRequiredSection("ScheduleSendServiceSettings:PollingIntervalCronUtc").Value!;

        services.AddSingleton(_ => new ScheduleSendServiceSettings(intervalString));

        services.AddSingleton<IScheduleSendService, ScheduleSendService>();

        return services;
    }
}