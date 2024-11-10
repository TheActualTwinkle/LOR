using DatabaseApp.Caching.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseApp.Caching;

public static class DependencyInjection
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
            options.Configuration = configuration.GetConnectionString("Redis") ?? throw new NullReferenceException("Can`t find Redis connection string in configuration"));
        
        services.AddSingleton<ICacheService>(provider =>
        {
            var distributedCache = provider.GetRequiredService<IDistributedCache>();
            
            return new CacheService(distributedCache, new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        });
        
        return services;
    }
}