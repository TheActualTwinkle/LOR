using System.Data;
using DatabaseApp.Application.Services.ReminderService;
using DatabaseApp.Application.Services.RemovalService;
using DatabaseApp.Caching.Decorators;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace DatabaseApp.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireService(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnectionString = configuration.GetConnectionString("HangfireDb");

        services.AddHangfire(c => c.UseDynamicJobs()
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(hangfireConnectionString))
            .UseFilter(new AutomaticRetryAttribute { Attempts = 3 }));

        services.AddHangfireServer(o =>
        {
            o.Queues = ["dba_queue"];
            o.SchedulePollingInterval = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    public static IServiceCollection AddOtel(this IServiceCollection services, IConfiguration configuration)
    {
        var resource = ResourceBuilder.CreateDefault()
            .AddService("DatabaseApp")
            .AddEnvironmentVariableDetector()
            .Build();

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;

                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity.SetTag("db.statement", command.CommandText);
                            activity.SetTag("db.parameters", GetSqlParameters(command));
                        };
                    })
                    .AddSource("MediatR")
                    .AddSource("MediatR.Validation")
                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                    .AddSource(TracingCacheDecorator.ActivitySource.Name)
                    .AddDomainServicesInstrumentation();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddProcessInstrumentation();
            })
            .ConfigureResource(rb => rb.AddAttributes(resource.Attributes))
            .UseOtlpExporter();

        services.AddSerilog(cl => cl
            .ReadFrom.Configuration(configuration)
            .WriteTo.OpenTelemetry(c =>
            {
                c.ResourceAttributes = resource.Attributes
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value);
            }));

        return services;
    }

    private static TracerProviderBuilder AddDomainServicesInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(ClassReminderService.ActivitySource.Name);
        builder.AddSource(ClassRemovalService.ActivitySource.Name);

        return builder;
    }

    private static string GetSqlParameters(IDbCommand command)
    {
        var parameters = new List<string>();

        foreach (IDataParameter p in command.Parameters)
            parameters.Add($"{p.ParameterName}={p.Value}");

        return string.Join(", ", parameters);
    }
}