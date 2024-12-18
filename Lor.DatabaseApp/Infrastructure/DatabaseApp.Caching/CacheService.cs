﻿using System.Text.Json;
using DatabaseApp.Caching.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DatabaseApp.Caching;

public class CacheService(IDistributedCache distributedCache, DistributedCacheEntryOptions options) : ICacheService
{
    private TimeSpan? DefaultExpirationRelativeToNow => options.AbsoluteExpirationRelativeToNow;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var value = await distributedCache.GetStringAsync(key, cancellationToken);
        return value == null ? null : JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default) where T : class =>
        await distributedCache.SetStringAsync(key,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime ?? DefaultExpirationRelativeToNow },
            cancellationToken);
}