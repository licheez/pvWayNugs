using pvNugsLoggerNc10Abstractions;

namespace pvNugsSecretManagerNc10;

/// <summary>
/// Wraps provider-level exceptions raised during secret retrieval operations,
/// providing a single, stable exception type for consumers to handle
/// regardless of the active backend provider.
/// </summary>
/// <remarks>
/// <para><c>Purpose:</c></para>
/// <para>
/// <see cref="PvNugsSecretManagerException"/> is thrown by the static secret retrieval
/// methods (<c>GetStaticSecretAsync</c>, <c>GetStaticSecretsAsync</c>) when the underlying
/// <see cref="pvNugsSecretManagerNc10Abstractions.IPvNugsSecretProvider"/> raises any exception.
/// This decouples consumer error-handling code from provider-specific exception hierarchies
/// (e.g. Azure SDK exceptions, HTTP exceptions, or Vault client exceptions).
/// </para>
///
/// <para><c>Message:</c></para>
/// <para>
/// The exception message is prefixed with <c>"pvNugsSecretManager "</c> followed by the
/// full deep message of the inner exception, produced by the
/// <c>GetDeepMessage</c> extension method from <c>pvNugsLoggerNc10Abstractions</c>.
/// This ensures that nested exception messages are surfaced rather than just the outermost one.
/// </para>
///
/// <para><c>Inner Exception:</c></para>
/// <para>
/// The original provider exception is always preserved as <see cref="Exception.InnerException"/>,
/// allowing detailed diagnostics, logging, and stack trace inspection when needed.
/// </para>
///
/// <para><c>Dynamic Credentials:</c></para>
/// <para>
/// Note that <c>GetDynamicSecretAsync</c> does <b>not</b> wrap exceptions in
/// <see cref="PvNugsSecretManagerException"/>; it propagates provider exceptions directly
/// to preserve transient-fault context for retry and circuit-breaker logic.
/// </para>
///
/// <para><c>Typical Handling Pattern:</c></para>
/// <code>
/// try
/// {
///     var secret = await secretManager.GetStaticSecretAsync(parameters, cancellationToken);
/// }
/// catch (PvNugsSecretManagerException ex)
/// {
///     // ex.InnerException holds the original provider-level exception
///     logger.LogError(ex, "Failed to retrieve secret. Inner: {Inner}", ex.InnerException?.Message);
///     throw;
/// }
/// catch (OperationCanceledException)
/// {
///     // Cancellation is not wrapped — handle separately
///     throw;
/// }
/// </code>
/// </remarks>
/// <seealso cref="pvNugsSecretManagerNc10Abstractions.IPvNugsSecretManager"/>
/// <seealso cref="pvNugsSecretManagerNc10Abstractions.IPvNugsSecretProvider"/>
public class PvNugsSecretManagerException(Exception e) :
    Exception($"pvNugsSecretManager {e.GetDeepMessage()}", e);
