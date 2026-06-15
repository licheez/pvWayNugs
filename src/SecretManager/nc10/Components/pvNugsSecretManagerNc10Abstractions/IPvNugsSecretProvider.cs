namespace pvNugsSecretManagerNc10Abstractions;

/// <summary>
/// Defines the provider-facing contract used by <see cref="IPvNugsSecretManager"/> to retrieve secrets
/// from a concrete backend such as Azure Key Vault, AWS Secrets Manager, HashiCorp Vault,
/// environment variables, or any custom secret source.
/// </summary>
/// <remarks>
/// <para><c>Architecture Role:</c></para>
/// <para>
/// Implementations of this interface encapsulate backend-specific access logic only.
/// They are responsible for translating a provider-specific parameters dictionary
/// into backend calls and returning normalized results to the manager layer.
/// </para>
///
/// <para><c>Separation of Concerns:</c></para>
/// <list type="bullet">
/// <item><description><c>Provider:</c> validates provider-specific parameters and talks to the remote/local backend.</description></item>
/// <item><description><c>Manager:</c> orchestrates provider calls, cross-cutting concerns, and optional caching policies.</description></item>
/// <item><description><c>Consumer:</c> selects the provider package and builds a dictionary with keys expected by that provider.</description></item>
/// </list>
///
/// <para><c>Parameter Dictionary Guidance:</c></para>
/// <list type="bullet">
/// <item><description>Use stable key names (prefer provider-specific constants) and clear value formats.</description></item>
/// <item><description>Required keys must be validated by the provider and fail fast when missing.</description></item>
/// <item><description>Unknown keys may be ignored or rejected based on provider policy, but behavior should be documented.</description></item>
/// <item><description>Sensitive values must never be logged in plain text.</description></item>
/// </list>
///
/// <para><c>Error Semantics:</c></para>
/// <para>
/// Providers should throw meaningful exceptions for invalid parameters, connectivity failures,
/// permission errors, and backend faults. A <see langword="null"/> return value is typically used to indicate
/// "not found" when that behavior is intentional and documented by the provider.
/// </para>
/// </remarks>
public interface IPvNugsSecretProvider
{
    /// <summary>
    /// Retrieves multiple static secret values from the underlying secret backend.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only dictionary containing resolved secret values.
    /// The exact output keys are provider-defined and should be documented by each implementation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parameters"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required provider-specific parameters are missing or invalid.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single static secret value from the underlying secret backend.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The resolved secret value, or <see langword="null"/> when no value is found and
    /// the provider treats that scenario as non-exceptional.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parameters"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required provider-specific parameters are missing or invalid.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<string?> GetStaticSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves dynamic, time-limited credentials from the underlying secret backend.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup arguments represented as immutable key/value pairs.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A dynamic credential object when available; otherwise <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Typical implementations include Vault-style credential leasing or backend-generated
    /// temporary usernames/passwords with explicit expiration timestamps.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parameters"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required provider-specific parameters are missing or invalid.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);
}