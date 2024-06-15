using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Types;
using TelegramBotApp.AppCommunication;
using TelegramBotApp.Application;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.AppCommunication.Interfaces;

namespace TelegramBotApp.Api.AppPipeline;

public class DefaultAppPipeline : IAppPipeline
{
    public async Task Run()
    {
        try
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                    config.AddJsonFile("appsettings.json", false, true))
                .ConfigureAppConfiguration(config =>
                    config.AddJsonFile("ScheduleSettings/launchSettings.json", false, true).AddEnvironmentVariables())
                
                .ConfigureServices((builder, services) => services
                    .AddApplication(builder.Configuration)
                    .AddCommunicators(builder.Configuration))
                .Build();
            
            await InitializeAppCommunicators([
                host.Services.GetRequiredService<IDatabaseCommunicator>(),
                host.Services.GetRequiredService<IGroupScheduleCommunicator>()
            ]);
            
            ITelegramBot botClient = host.Services.GetRequiredService<ITelegramBot>();
            IGroupScheduleCommunicator scheduleCommunicator = host.Services.GetRequiredService<IGroupScheduleCommunicator>();

            CancellationTokenSource cancellationToken = new();
            botClient.StartReceiving(scheduleCommunicator, cancellationToken.Token);

            User me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");

            // strange loop for simulation ReadLine
            while (cancellationToken.IsCancellationRequested == false)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken.Token);
            }

            await cancellationToken.CancelAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task InitializeAppCommunicators(IEnumerable<IAppCommunicator> communicators)
    {
        foreach (IAppCommunicator appCommunicator in communicators)
        {
            await appCommunicator.Start();
        }
    }
}