namespace DatabaseApp.Caching.Interfaces;

public interface ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    public Task SetAsync<T>(string key, T value, TimeSpan expirationTime, CancellationToken cancellationToken = default) where T : class;
}