using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.ScheduleUpdating.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GroupScheduleApp.ScheduleUpdating;

public static class DependencyInjection
{
    public static IServiceCollection AddSenderService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IScheduleSendService>(s =>
        {
            var logger = s.GetRequiredService<ILogger<ScheduleSendService>>();
            
            var scheduleProvider = s.GetRequiredService<IScheduleProvider>();
            var databaseUpdaterCommunicationClient = s.GetRequiredService<IDatabaseUpdaterCommunicationClient>();

            var intervalString = configuration.GetRequiredSection("ScheduleSendServiceSettings:PollingIntervalMinutes").Value!;
            var pollingInterval = TimeSpan.FromMinutes(int.Parse(intervalString));

            ScheduleSendServiceSettings settings = new(pollingInterval);
            
            return new ScheduleSendService(scheduleProvider, databaseUpdaterCommunicationClient, settings, logger);
        });
        return services;
    }
}