using DatabaseApp.AppCommunication;
using DatabaseApp.AppCommunication.Grpc;
using DatabaseApp.Application;
using DatabaseApp.Persistence;
using DatabaseApp.Persistence.DatabaseContext;
using DatabaseApp.WebApi.AppPipeline.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatabaseApp.WebApi.AppPipeline;

public class DefaultAppPipeline : IAppPipeline
{
    public async Task Run()
    {
        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
                config.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables())
            .Build();

        // DB где-то тут.

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        
        builder.Services.AddGrpc();
        builder.Services.AddApplication();
        builder.Services.AddCommunication();
        builder.Services.AddPersistence(builder.Configuration);

        WebApplication app = builder.Build();
        
        try
        {
            using IServiceScope scope = app.Services.CreateScope();
            IDatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
            await databaseContext.Db.MigrateAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }

        app.MapGrpcService<GrpcDatabaseService>();
        app.MapGrpcService<GrpcDatabaseUpdaterService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        await app.RunAsync();
    }
}