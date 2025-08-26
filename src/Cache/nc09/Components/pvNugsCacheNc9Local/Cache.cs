using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using pvNugsCacheNc9Abstractions;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsCacheNc9Local;

internal class Cache(
    ILoggerService logger,
    IMemoryCache cache,
    IOptions<PvNugsCacheConfig> options) : IPvNugsCache
{
    private readonly TimeSpan? _defaultTimeToLive = options.Value.DefaultTimeToLive;

    public async Task SetAsync<TValue>(
        string key, TValue value,
        TimeSpan? timeToLive = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var effectiveTimeToLive = timeToLive ?? _defaultTimeToLive;

        var cacheEntryOptions = new MemoryCacheEntryOptions();

        if (effectiveTimeToLive.HasValue)
        {
            cacheEntryOptions.SetAbsoluteExpiration(effectiveTimeToLive.Value);
        }

        cache.Set(key, value, cacheEntryOptions);

        await logger.LogAsync($"Cached item with key: {key}", SeverityEnu.Trace);

        await Task.CompletedTask;
    }

    public async Task<TValue?> GetAsync<TValue>(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key)) return default;

        var result = cache.TryGetValue(key, out var value)
                     && value is TValue typedValue
            ? typedValue
            : default;

        if (result == null)
            await logger.LogAsync($"Cache miss for key: {key}", SeverityEnu.Trace);
        else await logger.LogAsync($"Cache hit for key: {key}", SeverityEnu.Trace);

        return await Task.FromResult(result);
    }

    public async Task RemoveAsync(
        string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            return;

        cache.Remove(key);

        await logger.LogAsync($"Removed item with key: {key}", SeverityEnu.Trace);

        await Task.CompletedTask;
    }
}