using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace DatabaseApp.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddOtel(this IServiceCollection services, IConfiguration configuration)
    {
        var resource = ResourceBuilder.CreateDefault()
            .AddService("DatabaseApp")
            .Build();

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddMassTransitInstrumentation()
                    .AddGrpcCoreInstrumentation();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddProcessInstrumentation()
                    .AddRuntimeInstrumentation();
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
}