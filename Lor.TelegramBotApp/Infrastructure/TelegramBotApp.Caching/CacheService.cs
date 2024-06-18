using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TelegramBotApp.Caching.Interfaces;

namespace TelegramBotApp.Caching;

public class CacheService(IDistributedCache distributedCache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        string? value = await distributedCache.GetStringAsync(key, cancellationToken);
        return value == null ? null : JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        await distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await distributedCache.RemoveAsync(key, cancellationToken);
    }
}