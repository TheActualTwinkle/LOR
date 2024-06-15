using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public static class DependencyInjection
{
    public static IServiceCollection AddCommunicators(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseCommunicator, GrpcDatabaseCommunicator>();
        // TODO: Uncomment when IDatabaseCommunicator is implemented.
        // services.AddSingleton<IDatabaseCommunicator>(_ =>
        // {
        //     string hostname = configuration.GetSection("DatabaseCommunicator:Hostname").Value ?? throw new InvalidOperationException("Hostname is not set for IDatabaseCommunicator.");
        //     string portString = configuration.GetSection("DatabaseCommunicator:Port").Value ?? throw new InvalidOperationException("Port is not set for IDatabaseCommunicator.");
        //     return new GrpcDatabaseCommunicator();
        // });
        
        services.AddSingleton<IGroupScheduleCommunicator>(_ =>
        {
            string url = configuration.GetSection("profiles:GroupSchedule-http:applicationUrl").Value ?? throw new InvalidOperationException("GroupScheduleService url is not set.");
            return new GrpcGroupScheduleCommunicator(url);
        });

        return services;
    }
}