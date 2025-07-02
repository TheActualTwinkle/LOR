using DatabaseApp.Messaging;
using DatabaseApp.Application;
using DatabaseApp.Caching;
using DatabaseApp.Persistence;
using DatabaseApp.Persistence.DatabaseContext;
using DatabaseApp.WebApi.GrpcServices;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace DatabaseApp.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables();

        builder.Services.AddSerilog(cl => cl
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.OpenTelemetry(c => c.ResourceAttributes.Add("service.name", "DatabaseApp")));

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("DatabaseApp"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddGrpcCoreInstrumentation()
                    .AddRedisInstrumentation();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            })
            .UseOtlpExporter();
        
        // Order of services registration is important!!!
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();
        builder.Services.AddApplication();
        builder.Services.AddCaching(builder.Configuration);
        builder.Services.AddPersistence(builder.Configuration);
        builder.Services.AddBus(builder.Configuration);
        builder.Services.AddJobSchedule(builder.Configuration);

        var app = builder.Build();
        
        try
        {
            using var scope = app.Services.CreateScope();
            var databaseContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
            await databaseContext.Db.MigrateAsync();
        }
        catch (Exception e)
        {
            Log.Fatal("Error on DB migration: {message}", e.Message);
            throw;
        }

        app.MapGrpcService<GrpcDatabaseService>();
        app.MapGrpcService<GrpcDatabaseUpdaterService>();
        
        if (app.Environment.IsDevelopment())
            app.MapGrpcReflectionService();

        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. " +
                              "To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        await app.RunAsync();
    }
}