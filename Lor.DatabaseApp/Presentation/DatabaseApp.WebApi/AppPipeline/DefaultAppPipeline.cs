﻿using DatabaseApp.AppCommunication.Grpc;
using DatabaseApp.WebApi.AppPipeline.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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

        WebApplication app = builder.Build();

        app.MapGrpcService<GrpcDatabaseService>();
        app.MapGrpcService<GrpcDatabaseUpdaterService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        await app.RunAsync();
    }
}