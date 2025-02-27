using GroupScheduleApp.ScheduleProviding.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroupScheduleApp.ScheduleProviding;

public static class DependencyInjection
{
    public static IServiceCollection AddScheduleProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IScheduleProvider>(_ => new NstuHtmlScheduleProvider(GetClasses(configuration),
            configuration.GetRequiredSection("HtmlParser").GetValue<TimeSpan>("ScheduleFetchDateOffset")));
        return services;
    }
    
    private static IEnumerable<string> GetClasses(IConfiguration configuration) =>
        configuration.GetSection("HtmlParser:NSTU").Get<IEnumerable<string>>() ?? 
        throw new Exception("Schedule urls not found in configuration");
}