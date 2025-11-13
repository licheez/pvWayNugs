using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using pvNugsCacheNc6Abstractions;
using pvNugsLoggerNc6Abstractions;

namespace pvNugsCacheNc6Local;

internal class Cache : IPvNugsCache
{
    private readonly TimeSpan? _defaultTimeToLive;
    private readonly ILoggerService _logger;
    private readonly IMemoryCache _cache;

    public Cache(ILoggerService logger,
        IMemoryCache cache,
        IOptions<PvNugsCacheConfig> options)
    {
        _logger = logger;
        _cache = cache;
        _defaultTimeToLive = options.Value.DefaultTimeToLive;
    }

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

        _cache.Set(key, value, cacheEntryOptions);

        await _logger.LogAsync($"Cached item with key: {key}", SeverityEnu.Trace);

        await Task.CompletedTask;
    }

    public async Task<TValue?> GetAsync<TValue>(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key)) return default;

        var result = _cache.TryGetValue(key, out var value)
                     && value is TValue typedValue
            ? typedValue
            : default;

        if (result == null)
            await _logger.LogAsync($"Cache miss for key: {key}", SeverityEnu.Trace);
        else await _logger.LogAsync($"Cache hit for key: {key}", SeverityEnu.Trace);

        return await Task.FromResult(result);
    }

    public async Task RemoveAsync(
        string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            return;

        _cache.Remove(key);

        await _logger.LogAsync($"Removed item with key: {key}", SeverityEnu.Trace);

        await Task.CompletedTask;
    }
}