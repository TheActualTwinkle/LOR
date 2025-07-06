using GroupScheduleApp.Api.AppPipeline.Interfaces;
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
        using var host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
            .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json", false, true);
                    config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true);
                    config.AddEnvironmentVariables();
                })
            
            // Order of services registration is important!!!
            .ConfigureServices((builder, services) => services
                .AddCommunicators(builder.Configuration)
                .AddScheduleProvider(builder.Configuration)
                .AddJobScheduler(builder.Configuration)
            )
            .Build();

        var communicationClient = host.Services.GetRequiredService<IDatabaseUpdaterCommunicationClient>();
        var sendService = host.Services.GetRequiredService<IScheduleSendService>();
        
        await InitializeAppCommunicators([
            communicationClient
        ]);
        
        await sendService.StartAsync();
        
        await host.RunAsync();
    }
    
    private async Task InitializeAppCommunicators(IEnumerable<ICommunicationClient> communicators)
    {
        foreach (var appCommunicator in communicators)
            await appCommunicator.StartAsync();
    }
}