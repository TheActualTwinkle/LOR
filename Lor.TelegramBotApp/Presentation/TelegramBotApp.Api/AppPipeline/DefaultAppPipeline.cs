﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Types;
using TelegramBotApp.Api.AppPipeline.Interfaces;
using TelegramBotApp.AppCommunication;
using TelegramBotApp.Application;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Authorization;
using TelegramBotApp.Authorization.Interfaces;
using TelegramBotApp.Caching;
using TelegramBotApp.Caching.Interfaces;

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
                    config.AddJsonFile("DatabaseSettings/launchSettings.json", false, true).AddEnvironmentVariables())
                
                .ConfigureServices((builder, services) => services
                    .AddApplication(builder.Configuration)
                    .AddCommunicators(builder.Configuration)
                    .AddAuthorization()
                    .AddCaching(builder.Configuration))
                .Build();
            
            ITelegramBot botClient = host.Services.GetRequiredService<ITelegramBot>();
            IDatabaseCommunicationClient databaseCommunicator = host.Services.GetRequiredService<IDatabaseCommunicationClient>();
            IAuthorizationService authorizationService = host.Services.GetRequiredService<IAuthorizationService>();
            ICacheService cacheService = host.Services.GetRequiredService<ICacheService>();
            
            await InitializeAppCommunicators([
                databaseCommunicator
            ]);

            CancellationTokenSource cancellationToken = new();
            botClient.StartReceiving(databaseCommunicator, authorizationService, cacheService, cancellationToken.Token);

            User me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");

            await Task.Delay(Timeout.Infinite, cancellationToken.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task InitializeAppCommunicators(IEnumerable<ICommunicationClient> communicators)
    {
        foreach (ICommunicationClient appCommunicator in communicators)
        {
            await appCommunicator.Start();
        }
    }
}