namespace pvNugsCacheNc9Abstractions;

public interface IPvNugsCache
{
    Task SetAsync<TValue>(
        string key, TValue value,
        TimeSpan? timeToLive = null,
        CancellationToken cancellationToken = default);
    Task<TValue?> GetAsync<TValue>(
        string key, 
        CancellationToken cancellationToken = default);
    Task RemoveAsync(
        string key, CancellationToken cancellationToken = default);
}