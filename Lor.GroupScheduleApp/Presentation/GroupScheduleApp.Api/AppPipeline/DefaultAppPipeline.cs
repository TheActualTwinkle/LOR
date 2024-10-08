﻿using GroupScheduleApp.Api.AppPipeline.Interfaces;
using GroupScheduleApp.AppCommunication;
using GroupScheduleApp.AppCommunication.Interfaces;
using GroupScheduleApp.ScheduleProviding;
using GroupScheduleApp.ScheduleUpdating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GroupScheduleApp.Api.AppPipeline;

public class DefaultAppPipeline : IAppPipeline
{
    public async Task Run()
    {
        using IHost host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
            .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json", false, true);
                    config.AddJsonFile("DatabaseSettings/launchSettings.json", false, true);
                    config.AddEnvironmentVariables();
                })
            
            // Order of services registration is important!!!
            .ConfigureServices((builder, services) => services
                .AddLogging() // TODO: Check if needed
                .AddCommunicators(builder.Configuration)
                .AddScheduleProvider(builder.Configuration)
                .AddSenderService(builder.Configuration)
            )
            .Build();

        IDatabaseUpdaterCommunicationClient communicationClient = host.Services.GetRequiredService<IDatabaseUpdaterCommunicationClient>();
        IScheduleSendService sendService = host.Services.GetRequiredService<IScheduleSendService>();
        
        await InitializeAppCommunicators([
            communicationClient
        ]);
        
        await sendService.RunAsync();
    }
    
    private async Task InitializeAppCommunicators(IEnumerable<ICommunicationClient> communicators)
    {
        foreach (ICommunicationClient appCommunicator in communicators)
        {
            await appCommunicator.StartAsync();
        }
    }
}