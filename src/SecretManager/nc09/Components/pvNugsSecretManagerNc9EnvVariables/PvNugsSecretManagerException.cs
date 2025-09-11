namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Represents errors that occur during secret management operations within the pvNugs Secret Manager system.
/// This exception serves as a wrapper for underlying system exceptions, providing consistent error handling
/// and identification for secret management failures.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by the pvNugs Secret Manager components when errors occur during
/// secret retrieval, configuration access, or related operations. It wraps the original
/// exception to preserve the complete error context while providing a clear indication
/// that the error originated from the secret management system.
/// </para>
/// <para>
/// <strong>Common Scenarios:</strong>
/// </para>
/// <list type="bullet">
/// <item>Configuration provider failures (missing configuration files, network issues)</item>
/// <item>Invalid configuration section structures</item>
/// <item>Access permission issues when reading from configuration sources</item>
/// <item>Timeout errors during configuration retrieval</item>
/// <item>External secret provider communication failures</item>
/// </list>
/// <para>
/// <strong>Exception Handling Strategy:</strong>
/// </para>
/// <para>
/// This exception follows the standard .NET exception wrapping pattern, where the original
/// exception is preserved as the <see cref="Exception.InnerException"/>. This allows
/// consumers to access the full error context while still being able to catch and handle
/// secret management errors specifically.
/// </para>
/// <para>
/// <strong>Message Format:</strong>
/// </para>
/// <para>
/// Exception messages are formatted as "PvNugsSecretManager:{OriginalMessage}" to provide
/// clear identification of the error source while preserving the original error information.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// </para>
/// <para>
/// Like all .NET exceptions, instances of this class are immutable after construction
/// and are therefore thread-safe for read operations.
/// </para>
/// </remarks>
/// <param name="e">
/// The underlying exception that caused the secret management operation to fail.
/// This exception is wrapped and preserved as the <see cref="Exception.InnerException"/>,
/// maintaining the complete error context and stack trace information.
/// Must not be null.
/// </param>
/// <example>
/// <code>
/// // Typical usage pattern in secret manager implementation
/// try
/// {
///     var secret = configurationProvider.GetValue&lt;string&gt;(secretKey);
///     return secret;
/// }
/// catch (Exception ex)
/// {
///     await logger.LogAsync(ex);
///     throw new PvNugsSecretManagerException(ex);
/// }
/// 
/// // Consumer error handling
/// try
/// {
///     var credential = await secretManager.GetDynamicSecretAsync("DatabaseService");
///     // Use credential...
/// }
/// catch (PvNugsSecretManagerException ex)
/// {
///     // Handle secret management specific errors
///     logger.LogError(ex, "Failed to retrieve database credentials");
///     
///     // Access original exception if needed
///     if (ex.InnerException is ConfigurationException configEx)
///     {
///         // Handle configuration-specific errors
///         logger.LogError("Configuration error: {Error}", configEx.Message);
///     }
///     
///     // Implement fallback behavior
///     await HandleCredentialFailure();
/// }
/// catch (ArgumentException ex)
/// {
///     // Handle validation errors separately
///     logger.LogError(ex, "Invalid secret name provided");
/// }
/// 
/// // Logging example showing message format
/// // Log output: "PvNugsSecretManager:The configuration section 'MyApp' was not found."
/// 
/// // Exception details preservation
/// try
/// {
///     // Some operation that fails
///     throw new FileNotFoundException("Configuration file not found", "appsettings.json");
/// }
/// catch (Exception originalEx)
/// {
///     var wrappedException = new PvNugsSecretManagerException(originalEx);
///     
///     Console.WriteLine($"Message: {wrappedException.Message}");
///     // Output: "PvNugsSecretManager:Configuration file not found"
///     
///     Console.WriteLine($"Inner Exception: {wrappedException.InnerException?.GetType().Name}");
///     // Output: "FileNotFoundException"
///     
///     Console.WriteLine($"Original Stack Trace Preserved: {wrappedException.InnerException?.StackTrace != null}");
///     // Output: "True"
/// }
/// </code>
/// </example>
public class PvNugsSecretManagerException(Exception e): 
    Exception($"PvNugsSecretManager:{e.Message}", e);