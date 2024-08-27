namespace DatabaseApp.Caching.Interfaces;

public interface ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets the value of the key in the cache.
    /// </summary>
    /// <param name="expirationTime">No expiration time if NULL.</param>
    //ReSharper disable InvalidXmlDocComment
    public Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default) where T : class;
    //ReSharper restore InvalidXmlDocComment
}