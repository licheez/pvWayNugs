using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Extensions.Options;
using pvNugsCacheNc10Abstractions;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10;

/// <summary>
/// Provider-agnostic orchestration layer for secret retrieval.
/// Implements <see cref="IPvNugsSecretManager"/> by delegating backend calls to an injected
/// <see cref="IPvNugsSecretProvider"/> while applying cross-cutting concerns such as
/// transparent caching, structured logging, and exception normalization.
/// </summary>
/// <remarks>
/// <para><c>Architecture Role:</c></para>
/// <para>
/// <see cref="SecretManager"/> sits between the consumer application and the backend provider.
/// It is registered as a singleton and knows nothing about the concrete secret store;
/// all backend-specific logic lives in the <see cref="IPvNugsSecretProvider"/> implementation
/// that is injected at startup.
/// </para>
///
/// <para><c>Caching Strategy:</c></para>
/// <list type="bullet">
/// <item><description>
///   <c>Static secrets</c> — cached using the configurable TTL defined in
///   <see cref="PvNugsSecretManagerConfig.CacheTimeToLive"/> (default: 5 days).
///   The same policy applies to both single-secret and multi-secret retrieval.
/// </description></item>
/// <item><description>
///   <c>Dynamic credentials</c> — cached until their own
///   <see cref="IPvNugsDynamicCredential.ExpirationDateUtc"/>, so the cache entry
///   is guaranteed to expire before the credential itself becomes invalid.
///   Already-expired credentials are returned directly without caching.
/// </description></item>
/// </list>
///
/// <para><c>Cache Key Construction:</c></para>
/// <para>
/// Cache keys are deterministic strings built from the configured
/// <see cref="PvNugsSecretManagerConfig.CacheKeyPrefix"/> and the full parameter dictionary,
/// serialized as <c>prefix:key1=value1:key2=value2</c>.
/// Consumers must therefore pass parameters in a consistent order to guarantee cache hits
/// for identical logical requests.
/// </para>
///
/// <para><c>Exception Handling:</c></para>
/// <para>
/// All provider exceptions are caught, logged at <see cref="SeverityEnu.Error"/> severity,
/// and re-thrown as <see cref="PvNugsSecretManagerException"/> to give consumers a single,
/// stable exception type to handle regardless of the active provider.
/// Note that <c>GetDynamicSecretAsync</c> propagates provider exceptions directly (not wrapped)
/// because the dynamic credential flow should surface transient failures transparently.
/// </para>
///
/// <para><c>Logging:</c></para>
/// <para>
/// Each retrieval operation logs at <see cref="SeverityEnu.Trace"/> level so that secret
/// access patterns are observable without exposing secret values. Error conditions are
/// logged at <see cref="SeverityEnu.Error"/> level.
/// </para>
///
/// <para><c>Configuration:</c></para>
/// <para>
/// Behaviour is controlled via <see cref="PvNugsSecretManagerConfig"/>, which is bound from
/// the <c>appsettings.json</c> section named <c>PvNugsSecretManagerConfig</c>:
/// </para>
/// <code>
/// {
///   "PvNugsSecretManagerConfig": {
///     "CacheKeyPrefix": "MyApp",
///     "CacheTimeToLive": "1.00:00:00"
///   }
/// }
/// </code>
///
/// <para><c>Dependency Injection:</c></para>
/// <code>
/// // Register via the extension method provided by pvNugsSecretManagerNc10
/// services.AddPvNugsSecretManager&lt;MyAzureSecretProvider&gt;(configuration);
///
/// // Or manually:
/// services.Configure&lt;PvNugsSecretManagerConfig&gt;(
///     configuration.GetSection(PvNugsSecretManagerConfig.Section));
/// services.AddSingleton&lt;IPvNugsSecretProvider, MyAzureSecretProvider&gt;();
/// services.AddSingleton&lt;IPvNugsSecretManager, SecretManager&gt;();
/// </code>
/// </remarks>
/// <seealso cref="IPvNugsSecretManager"/>
/// <seealso cref="IPvNugsSecretProvider"/>
/// <seealso cref="IPvNugsDynamicCredential"/>
/// <seealso cref="PvNugsSecretManagerConfig"/>
/// <seealso cref="PvNugsSecretManagerException"/>
internal class SecretManager(
    IConsoleLoggerService logger,
    IPvNugsCache cache,
    IPvNugsSecretProvider provider,
    IOptions<PvNugsSecretManagerConfig> options) : IPvNugsSecretManager
{
    private readonly PvNugsSecretManagerConfig _config = options.Value;

    /// <summary>
    /// Retrieves multiple static secrets, serving from cache when available or delegating to
    /// the provider on a cache miss, then populating the cache for subsequent calls.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// The exact required and optional keys are defined by the active <see cref="IPvNugsSecretProvider"/>
    /// implementation. Refer to the provider package documentation for the expected dictionary shape.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cooperatively cancel the operation. Propagated to both the cache and provider calls.
    /// </param>
    /// <returns>
    /// A read-only dictionary of resolved static secret values keyed as defined by the active provider.
    /// The result is guaranteed non-null; an empty dictionary indicates the provider returned no values.
    /// </returns>
    /// <exception cref="PvNugsSecretManagerException">
    /// Thrown when the provider raises any exception. The original exception is preserved as
    /// <see cref="Exception.InnerException"/> and its deep message is included in the wrapper message.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(parameters);
        await logger.LogAsync(
            $"Getting static secrets with parameters: {cacheKey}",
            SeverityEnu.Trace);

        var cached = await cache.GetAsync<ReadOnlyDictionary<string, string>>(
            cacheKey, cancellationToken);
        if (cached != null) return cached;

        try
        {
            var res = await provider.GetStaticSecretsAsync(
                parameters, cancellationToken);
            await cache.SetAsync(cacheKey, res, _config.CacheTimeToLive, cancellationToken);
            return res;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e, SeverityEnu.Error);
            throw new PvNugsSecretManagerException(e);
        }
    }

    /// <summary>
    /// Retrieves a single static secret, serving from cache when available or delegating to
    /// the provider on a cache miss, then populating the cache for subsequent calls.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// The exact required and optional keys are defined by the active <see cref="IPvNugsSecretProvider"/>
    /// implementation. Refer to the provider package documentation for the expected dictionary shape.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cooperatively cancel the operation. Propagated to both the cache and provider calls.
    /// </param>
    /// <returns>
    /// The resolved secret value string, or <see langword="null"/> when the provider returns
    /// a <see langword="null"/> or empty value. A <see langword="null"/> return is not cached.
    /// </returns>
    /// <exception cref="PvNugsSecretManagerException">
    /// Thrown when the provider raises any exception. The original exception is preserved as
    /// <see cref="Exception.InnerException"/> and its deep message is included in the wrapper message.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task<string?> GetStaticSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(parameters);
        await logger.LogAsync(
            $"Getting static secret with parameters: {cacheKey}",
            SeverityEnu.Trace);
        var cached = await cache.GetAsync<string>(
            cacheKey, cancellationToken);
        if (cached != null) return cached;

        try
        {
            var res = await provider.GetStaticSecretAsync(
                parameters, cancellationToken);
            if (string.IsNullOrEmpty(res)) return null;
            await cache.SetAsync(cacheKey, res, _config.CacheTimeToLive, cancellationToken);
            return res;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsSecretManagerException(e);
        }
    }

    /// <summary>
    /// Retrieves dynamic, expiring credentials, serving from cache when a non-expired entry exists,
    /// or delegating to the provider and caching the result until its own expiration time.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// The exact required and optional keys are defined by the active <see cref="IPvNugsSecretProvider"/>
    /// implementation. Refer to the provider package documentation for the expected dictionary shape.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cooperatively cancel the operation. Propagated to both the cache and provider calls.
    /// </param>
    /// <returns>
    /// A valid <see cref="IPvNugsDynamicCredential"/> instance when the provider returns one,
    /// or <see langword="null"/> when no credential is available for the given parameters.
    /// </returns>
    /// <remarks>
    /// <para><c>Expiration-Aware Caching:</c></para>
    /// <para>
    /// When a credential is returned by the provider, its cache TTL is computed as
    /// <c>ExpirationDateUtc - DateTime.UtcNow</c>. Only credentials with a strictly positive
    /// remaining lifetime are cached; already-expired credentials are returned directly
    /// without a cache write, allowing the caller to detect and handle this edge case.
    /// This guarantees that a cached credential is never served past its expiry time.
    /// </para>
    ///
    /// <para><c>Exception Propagation:</c></para>
    /// <para>
    /// Unlike the static secret methods, provider exceptions are not wrapped in
    /// <see cref="PvNugsSecretManagerException"/> — they propagate directly to the caller.
    /// This preserves the transient-fault context which is important for retry and circuit-breaker
    /// logic in dynamic credential workflows.
    /// </para>
    /// </remarks>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    public async Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(parameters);
        await logger.LogAsync(
            $"Getting dynamic secret with parameters: {cacheKey}",
            SeverityEnu.Trace);

        var cached = await cache.GetAsync<IPvNugsDynamicCredential>(
            cacheKey, cancellationToken);
        if (cached != null) return cached;

        var res = await provider.GetDynamicSecretAsync(
            parameters, cancellationToken);

        if (res == null) return null;

        // Only cache credentials that still have a positive remaining lifetime.
        // TTL is derived from the credential's own expiration so the cache entry
        // never outlives the credential itself.
        var ttl = res.ExpirationDateUtc - DateTime.UtcNow;
        if (ttl.TotalSeconds <= 0) return res;

        await cache.SetAsync(cacheKey, res, ttl, cancellationToken);
        return res;
    }

    /// <summary>
    /// Builds a deterministic cache key from the configured prefix and the full parameter dictionary.
    /// </summary>
    /// <param name="parameters">
    /// The parameters dictionary whose entries are serialised into the key.
    /// </param>
    /// <returns>
    /// A string of the form <c>prefix:key1=value1:key2=value2</c> derived from the
    /// <see cref="PvNugsSecretManagerConfig.CacheKeyPrefix"/> and the ordered entries of
    /// <paramref name="parameters"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Key uniqueness depends on the iteration order of <paramref name="parameters"/>.
    /// When using a standard <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/>,
    /// insertion order is preserved in .NET Core; however, consumers should use a consistent
    /// entry order across calls to guarantee cache hits for logically identical requests.
    /// </para>
    /// </remarks>
    private string GetCacheKey(IReadOnlyDictionary<string, string> parameters)
    {
        var sb = new StringBuilder(_config.CacheKeyPrefix);
        foreach (var kvp in parameters)
        {
            sb.Append($":{kvp.Key}={kvp.Value}");
        }
        return sb.ToString();
    }
}

