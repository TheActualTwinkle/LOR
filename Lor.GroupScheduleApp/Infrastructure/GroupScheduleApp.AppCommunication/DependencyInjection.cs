using GroupScheduleApp.AppCommunication.Grpc;
using GroupScheduleApp.AppCommunication.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GroupScheduleApp.AppCommunication;

public static class DependencyInjection
{
    public static IServiceCollection AddCommunicators(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseUpdaterCommunicationClient, GrpcDatabaseUpdaterClient>(s =>
        {
            var logger = s.GetRequiredService<ILogger<GrpcDatabaseUpdaterClient>>();
            
            var url = configuration.GetSection("DatabaseApp:Url").Value ?? throw new InvalidOperationException("GrpcDatabaseCommunicationClient url is not set.");
            return new GrpcDatabaseUpdaterClient(url, logger);
        });
        return services;
    }
}