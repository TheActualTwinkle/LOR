using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TelegramBotApp.Api.AppPipeline.Interfaces;
using TelegramBotApp.AppCommunication;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application;
using TelegramBotApp.Authorization;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.Api.AppPipeline;

public class DefaultAppPipeline : IAppPipeline
{
    public async Task Run()
    {
        try
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
                    .AddNstuAuthorization()                  
                    .AddApplication(builder.Configuration)
                    .AddBus(builder.Configuration)
                )
                .Build();
            
            var botClient = host.Services.GetRequiredService<ITelegramBot>();
            var databaseCommunicator = host.Services.GetRequiredService<IDatabaseCommunicationClient>();
            
            await InitializeAppCommunicators([
                databaseCommunicator
            ]);

            CancellationTokenSource cancellationToken = new();
            botClient.StartReceiving(cancellationToken.Token);

            var me = await botClient.GetMeAsync();
            Log.Information("Start listening for @{username}", me.Username);

            await host.RunAsync(cancellationToken.Token);
        }
        catch (Exception e)
        {
            Log.Fatal(e.Message);
            throw;
        }
    }

    private async Task InitializeAppCommunicators(IEnumerable<ICommunicationClient> communicators)
    {
        foreach (var appCommunicator in communicators)
            await appCommunicator.StartAsync();
    }
}