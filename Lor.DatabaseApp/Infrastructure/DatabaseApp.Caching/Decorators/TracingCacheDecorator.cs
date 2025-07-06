using System.Diagnostics;
using DatabaseApp.Caching.Interfaces;

namespace DatabaseApp.Caching.Decorators;

public class TracingCacheDecorator(ICacheService inner) : ICacheService
{
    public static readonly ActivitySource ActivitySource = new("Caching");

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        using var activity = ActivitySource.StartActivity($"Cache.Get AS {typeof(T).Name}");

        activity?.SetTag("cache.key", key);

        try
        {
            var result = await inner.GetAsync<T>(key, cancellationToken);

            activity?.SetTag("cache.hit", result is not null);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default) where T : class
    {
        using var activity = ActivitySource.StartActivity($"Cache.Set AS {typeof(T).Name}");

        activity?.SetTag("cache.key", key);

        try
        {
            await inner.SetAsync(key, value, expirationTime, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }
}