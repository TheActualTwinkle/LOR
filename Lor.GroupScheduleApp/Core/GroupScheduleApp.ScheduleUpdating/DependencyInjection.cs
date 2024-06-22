using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.ScheduleProviding.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GroupScheduleApp.ScheduleUpdating;

public static class DependencyInjection
{
    public static IServiceCollection AddSenderService(this IServiceCollection services)
    {
        services.AddSingleton<IScheduleSenderService>(s =>
        {
            IScheduleProvider scheduleProvider = s.GetRequiredService<IScheduleProvider>();
            return new ScheduleSendService(scheduleProvider, s.GetRequiredService<IDatabaseUpdaterCommunicationClient>());
        });
        return services;
    }
}