using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotApp.Caching.Interfaces;

namespace TelegramBotApp.Caching;

public static class DependencyInjection
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
                options.Configuration = configuration.GetConnectionString("Redis") ?? throw new NullReferenceException("Can`t find Redis connection string in configuration"))
            .AddSingleton<ICacheService, CacheService>();

        return services;
    }
}