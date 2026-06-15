namespace pvNugsSecretManagerNc10Abstractions;

/// <summary>
/// Defines the consumer-facing secret management contract.
/// </summary>
/// <remarks>
/// <para><c>Purpose:</c></para>
/// <para>
/// This interface is the stable entry point used by application code.
/// Implementations typically delegate retrieval to an injected <see cref="IPvNugsSecretProvider"/>
/// while applying cross-cutting concerns such as caching, retries, telemetry, and policy enforcement.
/// </para>
///
/// <para><c>Provider Independence:</c></para>
/// <para>
/// Consumers depend only on this contract and remain agnostic to the actual backend provider
/// (Azure, AWS, HashiCorp Vault, environment variables, and so on).
/// Backend selection is resolved via dependency injection.
/// </para>
///
/// <para><c>Parameter Model:</c></para>
/// <para>
/// Method calls accept a provider-specific <see cref="IReadOnlyDictionary{TKey,TValue}"/> so the contract
/// remains extensible without interface churn. Consumers are expected to build dictionaries
/// according to the selected provider's documented key contract.
/// </para>
///
/// <para><c>Caching Guidance:</c></para>
/// <list type="bullet">
/// <item><description>Static secrets are typically cache candidates when freshness requirements allow it.</description></item>
/// <item><description>Dynamic credentials should consider expiration-aware caching and renewal windows.</description></item>
/// <item><description>Cache keys should be deterministic for equivalent parameter dictionaries.</description></item>
/// </list>
/// </remarks>
public interface IPvNugsSecretManager
{
    /// <summary>
    /// Retrieves multiple static secrets using provider-specific parameters.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only dictionary of resolved static secret values.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parameters"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required parameters are missing or malformed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single static secret value using provider-specific parameters.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The resolved secret value, or <see langword="null"/> when no value is found and
    /// the selected implementation treats that result as non-exceptional.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parameters"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required parameters are missing or malformed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<string?> GetStaticSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves dynamic, expiring credentials using provider-specific parameters.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A dynamic credential instance when available; otherwise <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Manager implementations should preserve expiration semantics and may optionally apply
    /// short-lived caching strategies that never outlive <see cref="IPvNugsDynamicCredential.ExpirationDateUtc"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parameters"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required parameters are missing or malformed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);
}