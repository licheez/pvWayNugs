namespace pvNugsCacheNc6Abstractions;

/// <summary>
/// Defines a contract for caching operations with asynchronous support.
/// Provides methods to store, retrieve, and remove cached values with optional time-to-live settings.
/// </summary>
public interface IPvNugsCache
{
    /// <summary>
    /// Asynchronously stores a value in the cache with the specified key.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to cache.</typeparam>
    /// <param name="key">The unique identifier for the cached value.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="timeToLive">Optional expiration time for the cached value. If null, uses default cache expiration policy.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous cache operation.</returns>
    Task SetAsync<TValue>(
        string key, TValue value,
        TimeSpan? timeToLive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a value from the cache using the specified key.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
    /// <param name="key">The unique identifier for the cached value.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous retrieval operation.
    /// The task result contains the cached value if found, or null if the key doesn't exist or has expired.
    /// </returns>
    Task<TValue?> GetAsync<TValue>(
        string key, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously removes a value from the cache using the specified key.
    /// </summary>
    /// <param name="key">The unique identifier for the cached value to remove.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous removal operation.</returns>
    Task RemoveAsync(
        string key, CancellationToken cancellationToken = default);
}