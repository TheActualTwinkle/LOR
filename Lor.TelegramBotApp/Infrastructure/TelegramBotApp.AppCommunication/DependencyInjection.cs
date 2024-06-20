using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.AppCommunication;

public static class DependencyInjection
{
    public static IServiceCollection AddCommunicators(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseCommunicationClient, GrpcDatabaseClient>(_ =>
        {
            string url = configuration.GetSection("profiles:Database-http:applicationUrl").Value ?? throw new InvalidOperationException("GrpcDatabaseCommunicationClient url is not set.");
            return new GrpcDatabaseClient(url);
        });
        return services;
    }
}