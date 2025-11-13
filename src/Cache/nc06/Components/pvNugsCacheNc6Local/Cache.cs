using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using pvNugsCacheNc6Abstractions;
using pvNugsLoggerNc6Abstractions;

namespace pvNugsCacheNc6Local;

/// <summary>
/// In-memory cache implementation using Microsoft.Extensions.Caching.Memory.
/// Provides a thread-safe, high-performance caching solution with configurable time-to-live settings.
/// </summary>
internal class Cache : IPvNugsCache
{
    private readonly TimeSpan? _defaultTimeToLive;
    private readonly ILoggerService _logger;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the Cache class.
    /// </summary>
    /// <param name="logger">Logger service for tracing cache operations</param>
    /// <param name="cache">The underlying IMemoryCache instance</param>
    /// <param name="options">Configuration options including default time-to-live settings</param>
    public Cache(ILoggerService logger,
        IMemoryCache cache,
        IOptions<PvNugsCacheConfig> options)
    {
        _logger = logger;
        _cache = cache;
        _defaultTimeToLive = options.Value.DefaultTimeToLive;
    }

    /// <summary>
    /// Stores a value in the cache with an optional time-to-live.
    /// If no time-to-live is specified, uses the default configuration value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to cache</typeparam>
    /// <param name="key">The unique cache key. Cannot be null or empty.</param>
    /// <param name="value">The value to store in the cache</param>
    /// <param name="timeToLive">Optional time-to-live duration. If null, uses the default from configuration.</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown when the key is null or empty</exception>
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

    /// <summary>
    /// Retrieves a value from the cache by its key.
    /// Returns the default value for the type if the key is not found or the value is of an incompatible type.
    /// </summary>
    /// <typeparam name="TValue">The expected type of the cached value</typeparam>
    /// <param name="key">The cache key to retrieve. Returns default if null or empty.</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The cached value if found and of the correct type; otherwise, the default value for TValue</returns>
    public async Task<TValue?> GetAsync<TValue>(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key)) return default;

        var result = _cache.TryGetValue(key, out var value)
                     && value is TValue typedValue
            ? typedValue
            : default;

        var message = EqualityComparer<TValue>.Default.Equals(result, default)
            ? $"Cache miss for key: {key}"
            : $"Cache hit for key: {key}";
        
        await _logger.LogAsync(message, SeverityEnu.Trace);

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Removes a cached item by its key.
    /// If the key is null or empty, the operation is ignored without throwing an exception.
    /// </summary>
    /// <param name="key">The cache key to remove. Ignored if null or empty.</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>A task representing the asynchronous operation</returns>
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