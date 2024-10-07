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
            ILogger<GrpcDatabaseUpdaterClient> logger = s.GetRequiredService<ILogger<GrpcDatabaseUpdaterClient>>();
            
            string url = configuration.GetSection("profiles:Database-http:applicationUrl").Value ?? throw new InvalidOperationException("GrpcDatabaseCommunicationClient url is not set.");
            return new GrpcDatabaseUpdaterClient(url, logger);
        });
        return services;
    }
}