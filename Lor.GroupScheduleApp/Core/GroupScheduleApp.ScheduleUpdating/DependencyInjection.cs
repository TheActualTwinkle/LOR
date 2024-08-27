using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.ScheduleProviding.Interfaces;
using GroupScheduleApp.ScheduleUpdating.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace GroupScheduleApp.ScheduleUpdating;

public static class DependencyInjection
{
    public static IServiceCollection AddSenderService(this IServiceCollection services)
    {
        services.AddSingleton<IScheduleSendService>(s =>
        {
            IScheduleProvider scheduleProvider = s.GetRequiredService<IScheduleProvider>();
            IDatabaseUpdaterCommunicationClient databaseUpdaterCommunicationClient = s.GetRequiredService<IDatabaseUpdaterCommunicationClient>();
            ScheduleSendServiceSettings settings = new(TimeSpan.FromDays(1));
            return new ScheduleSendService(scheduleProvider, databaseUpdaterCommunicationClient, settings);
        });
        return services;
    }
}