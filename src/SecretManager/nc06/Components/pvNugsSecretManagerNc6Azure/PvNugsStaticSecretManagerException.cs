using pvNugsSecretManagerNc6Abstractions;

namespace pvNugsSecretManagerNc6Azure;

/// <summary>
/// Represents errors that occur during Azure Key Vault secret management operations.
/// This exception serves as a wrapper for various underlying exceptions that may occur
/// during secret retrieval, authentication, or communication with Azure Key Vault services.
/// It provides a consistent exception contract for all secret management operations
/// while preserving the original exception details for debugging and troubleshooting.
/// </summary>
/// <remarks>
/// <para><c>Purpose and Design:</c></para>
/// <para>This exception class is designed to provide a unified error handling mechanism for all
/// Azure Key Vault secret management operations. It wraps various types of underlying exceptions
/// that may occur during the secret retrieval process, including network issues, authentication
/// failures, authorization problems, and Azure service errors.</para>
/// 
/// <para><c>Exception Wrapping Strategy:</c></para>
/// <para>The exception follows a consistent wrapping pattern where all underlying exceptions are preserved
/// as inner exceptions, allowing for detailed troubleshooting while providing a clean, predictable
/// exception type for application code to handle. The original exception message is enhanced with
/// a clear prefix to identify it as a secret management error.</para>
/// 
/// <para><c>Common Underlying Exception Types:</c></para>
/// <list type="bullet">
/// <item><description><c>Azure.RequestFailedException:</c> Azure Key Vault service errors, including authentication and authorization failures</description></item>
/// <item><description><c>System.Net.Http.HttpRequestException:</c> Network connectivity issues when communicating with Azure Key Vault</description></item>
/// <item><description><c>System.Security.Authentication.AuthenticationException:</c> Credential validation failures</description></item>
/// <item><description><c>System.UnauthorizedAccessException:</c> Insufficient permissions to access specified secrets</description></item>
/// <item><description><c>System.TimeoutException:</c> Request timeout when communicating with Azure Key Vault</description></item>
/// <item><description><c>System.ArgumentException:</c> Invalid secret names or configuration parameters</description></item>
/// <item><description><c>System.UriFormatException:</c> Malformed Key Vault URLs in configuration</description></item>
/// </list>
/// 
/// <para><c>Error Handling Best Practices:</c></para>
/// <list type="bullet">
/// <item><description>Always check the <see cref="Exception.InnerException"/> property for detailed error information</description></item>
/// <item><description>Log both the wrapper exception and inner exception details for comprehensive troubleshooting</description></item>
/// <item><description>Consider implementing retry logic for transient errors (network timeouts, service throttling)</description></item>
/// <item><description>Handle authentication errors by checking Azure credentials and permissions</description></item>
/// <item><description>Validate configuration settings when encountering format or argument exceptions</description></item>
/// </list>
/// 
/// <para><c>Security Considerations:</c></para>
/// <list type="bullet">
/// <item><description>Secret values are never included in exception messages to prevent accidental logging or exposure</description></item>
/// <item><description>Exception messages focus on operational issues rather than sensitive data</description></item>
/// <item><description>Inner exceptions may contain Azure SDK error details that should be logged securely</description></item>
/// <item><description>Consider sanitizing exception details before exposing them in user-facing error messages</description></item>
/// </list>
/// 
/// <para><c>Diagnostic Information:</c></para>
/// <para>When troubleshooting exceptions of this type, examine:</para>
/// <list type="bullet">
/// <item><description>The inner exception type and message for specific error details</description></item>
/// <item><description>Azure Key Vault access policies and permissions</description></item>
/// <item><description>Network connectivity to Azure Key Vault endpoints</description></item>
/// <item><description>Authentication credential validity and configuration</description></item>
/// <item><description>Key Vault URL format and accessibility</description></item>
/// <item><description>Secret name validity and existence in the Key Vault</description></item>
/// </list>
/// </remarks>
/// <example>
/// <para>Typical exception handling in application code:</para>
/// <code>
/// try
/// {
///     var secret = await secretManager.GetStaticSecretAsync("my-secret");
///     return secret;
/// }
/// catch (PvNugsStaticSecretManagerException ex)
/// {
///     // Log the full exception with inner exception details
///     logger.LogError(ex, "Failed to retrieve secret from Azure Key Vault");
///     
///     // Check inner exception for specific error handling
///     switch (ex.InnerException)
///     {
///         case Azure.RequestFailedException azureEx when azureEx.Status == 404:
///             throw new SecretNotFoundException("Secret not found in Key Vault", ex);
///             
///         case Azure.RequestFailedException azureEx when azureEx.Status == 403:
///             throw new UnauthorizedAccessException("Insufficient permissions to access Key Vault", ex);
///             
///         case HttpRequestException:
///             throw new ServiceUnavailableException("Key Vault service temporarily unavailable", ex);
///             
///         default:
///             throw new ApplicationException("Unexpected error during secret retrieval", ex);
///     }
/// }
/// </code>
/// 
/// <para>Retry logic for transient errors:</para>
/// <code>
/// public async Task&lt;string?&gt; GetSecretWithRetryAsync(string secretName, int maxRetries = 3)
/// {
///     for (int attempt = 1; attempt &lt;= maxRetries; attempt++)
///     {
///         try
///         {
///             return await secretManager.GetStaticSecretAsync(secretName);
///         }
///         catch (PvNugsStaticSecretManagerException ex) when (IsTransientError(ex) &amp;&amp; attempt &lt; maxRetries)
///         {
///             var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
///             logger.LogWarning(ex, "Transient error on attempt {Attempt}, retrying in {Delay}ms", 
///                              attempt, delay.TotalMilliseconds);
///             await Task.Delay(delay);
///         }
///     }
/// }
/// 
/// private static bool IsTransientError(PvNugsStaticSecretManagerException ex)
/// {
///     return ex.InnerException is HttpRequestException or TimeoutException or
///            (Azure.RequestFailedException azureEx &amp;&amp; azureEx.Status == 429); // Too Many Requests
/// }
/// </code>
/// 
/// <para>Configuration validation to prevent exceptions:</para>
/// <code>
/// public void ValidateConfiguration(PvNugsAzureSecretManagerConfig config)
/// {
///     try
///     {
///         if (string.IsNullOrWhiteSpace(config.KeyVaultUrl))
///             throw new ArgumentException("Key Vault URL is required");
///             
///         var uri = new Uri(config.KeyVaultUrl);
///         if (!uri.Host.EndsWith(".vault.azure.net", StringComparison.OrdinalIgnoreCase))
///             throw new ArgumentException("Invalid Key Vault URL format");
///             
///         if (config.Credential != null)
///         {
///             if (!Guid.TryParse(config.Credential.TenantId, out _))
///                 throw new ArgumentException("Invalid TenantId format");
///                 
///             if (!Guid.TryParse(config.Credential.ClientId, out _))
///                 throw new ArgumentException("Invalid ClientId format");
///                 
///             if (string.IsNullOrWhiteSpace(config.Credential.ClientSecret))
///                 throw new ArgumentException("ClientSecret is required when using service principal");
///         }
///     }
///     catch (Exception ex)
///     {
///         throw new PvNugsStaticSecretManagerException($"Configuration validation failed: {ex.Message}");
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="Exception"/>
/// <seealso cref="System.Exception.InnerException"/>
/// <seealso cref="IPvNugsStaticSecretManager"/>
/// <seealso cref="PvNugsStaticSecretManager"/>
/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions">Exception Best Practices</seealso>
public class PvNugsStaticSecretManagerException : 
    Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsStaticSecretManagerException"/> class
    /// with a specified error message. This constructor is typically used for application-level
    /// errors where a descriptive message provides sufficient context for troubleshooting.
    /// </summary>
    /// <param name="message">
    /// The message that describes the error. This message should be descriptive and provide
    /// sufficient context for understanding the nature of the secret management failure.
    /// The message will be prefixed with "PvNugsStaticSecretManagerException: " to clearly
    /// identify it as a secret management error.
    /// </param>
    /// <remarks>
    /// <para><c>Usage Scenarios:</c></para>
    /// <list type="bullet">
    /// <item><description>Configuration validation errors where the underlying cause is clear</description></item>
    /// <item><description>Business logic errors in secret management operations</description></item>
    /// <item><description>Application-specific error conditions that don't stem from underlying exceptions</description></item>
    /// <item><description>Parameter validation failures in secret management methods</description></item>
    /// </list>
    /// 
    /// <para><c>Message Formatting:</c></para>
    /// <para>The constructor automatically prefixes the provided message with "PvNugsStaticSecretManagerException: "
    /// to ensure consistent identification of secret management errors in logs and error handling code.
    /// This prefix helps distinguish these exceptions from other application exceptions.</para>
    /// 
    /// <para><c>Security Considerations:</c></para>
    /// <list type="bullet">
    /// <item><description>Ensure the message does not contain sensitive information such as secret values</description></item>
    /// <item><description>Focus on operational aspects rather than exposing internal implementation details</description></item>
    /// <item><description>Consider the audience that might see this message in logs or error reports</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="message"/> is null. However, empty or whitespace messages are allowed.
    /// </exception>
    /// <example>
    /// <para>Creating exceptions for configuration validation:</para>
    /// <code>
    /// if (string.IsNullOrWhiteSpace(keyVaultUrl))
    /// {
    ///     throw new PvNugsStaticSecretManagerException("Key Vault URL cannot be null or empty");
    /// }
    /// 
    /// if (!Uri.IsWellFormedUriString(keyVaultUrl, UriKind.Absolute))
    /// {
    ///     throw new PvNugsStaticSecretManagerException($"Invalid Key Vault URL format: {keyVaultUrl}");
    /// }
    /// </code>
    /// 
    /// <para>Creating exceptions for business logic errors:</para>
    /// <code>
    /// if (secretName.Length > MaxSecretNameLength)
    /// {
    ///     throw new PvNugsStaticSecretManagerException(
    ///         $"Secret name exceeds maximum length of {MaxSecretNameLength} characters");
    /// }
    /// 
    /// if (!IsValidSecretName(secretName))
    /// {
    ///     throw new PvNugsStaticSecretManagerException(
    ///         "Secret name contains invalid characters. Only alphanumeric characters and hyphens are allowed");
    /// }
    /// </code>
    /// 
    /// <para>Exception handling in calling code:</para>
    /// <code>
    /// try
    /// {
    ///     await secretManager.ValidateConfigurationAsync();
    /// }
    /// catch (PvNugsStaticSecretManagerException ex)
    /// {
    ///     // The message will be: "PvNugsStaticSecretManagerException: Key Vault URL cannot be null or empty"
    ///     logger.LogError(ex, "Configuration validation failed");
    ///     throw new InvalidOperationException("Secret manager configuration is invalid", ex);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="PvNugsStaticSecretManagerException(Exception)"/>
    public PvNugsStaticSecretManagerException(string message):
        base($"PvNugsStaticSecretManagerException: {message}")
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsStaticSecretManagerException"/> class
    /// with a reference to the inner exception that is the root cause of this exception.
    /// This constructor is the primary mechanism for wrapping underlying exceptions that occur
    /// during Azure Key Vault operations while preserving the original exception details.
    /// </summary>
    /// <param name="e">
    /// The exception that is the cause of the current exception. This parameter should contain
    /// the original exception that occurred during secret management operations, such as Azure SDK
    /// exceptions, network exceptions, authentication failures, or other system-level errors.
    /// The inner exception is preserved to maintain full diagnostic information.
    /// </param>
    /// <remarks>
    /// <para><c>Exception Wrapping Strategy:</c></para>
    /// <para>This constructor implements a consistent exception wrapping pattern where:</para>
    /// <list type="bullet">
    /// <item><description>The original exception message is preserved and prefixed with "PvNugsStaticSecretManagerException: "</description></item>
    /// <item><description>The complete inner exception is preserved for detailed troubleshooting</description></item>
    /// <item><description>Stack trace information from the original exception is maintained</description></item>
    /// <item><description>Exception data and other properties from the inner exception remain accessible</description></item>
    /// </list>
    /// 
    /// <para><c>Common Inner Exception Types:</c></para>
    /// <para>This constructor is typically used to wrap the following types of exceptions:</para>
    /// <list type="bullet">
    /// <item><description><c>Azure.RequestFailedException:</c> Azure Key Vault service errors (404, 403, 401, 429, 500, etc.)</description></item>
    /// <item><description><c>System.Net.Http.HttpRequestException:</c> HTTP communication failures</description></item>
    /// <item><description><c>System.Security.Authentication.AuthenticationException:</c> Authentication credential failures</description></item>
    /// <item><description><c>System.TimeoutException:</c> Request timeout scenarios</description></item>
    /// <item><description><c>System.UnauthorizedAccessException:</c> Permission and authorization issues</description></item>
    /// <item><description><c>System.ArgumentException:</c> Parameter validation failures from Azure SDK</description></item>
    /// <item><description><c>System.UriFormatException:</c> Key Vault URL formatting issues</description></item>
    /// </list>
    /// 
    /// <para><c>Diagnostic Information Preservation:</c></para>
    /// <list type="bullet">
    /// <item><description>Original error codes and status information are preserved in the inner exception</description></item>
    /// <item><description>Azure-specific error details and request IDs remain accessible for support scenarios</description></item>
    /// <item><description>Complete stack trace from the point of the original failure is maintained</description></item>
    /// <item><description>Exception data collections and custom properties are preserved</description></item>
    /// </list>
    /// 
    /// <para><c>Troubleshooting Benefits:</c></para>
    /// <list type="bullet">
    /// <item><description>Enables detailed analysis of the root cause while providing a consistent exception type</description></item>
    /// <item><description>Supports both automated error handling and manual troubleshooting scenarios</description></item>
    /// <item><description>Maintains compatibility with existing exception handling patterns</description></item>
    /// <item><description>Provides context for both application developers and Azure support teams</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="e"/> is null. All exception wrapping scenarios require a valid inner exception.
    /// </exception>
    /// <example>
    /// <para>Typical usage in secret manager implementation:</para>
    /// <code>
    /// public async Task&lt;string?&gt; GetStaticSecretAsync(string secretName)
    /// {
    ///     try
    ///     {
    ///         // Azure Key Vault operation
    ///         var secret = await keyVaultClient.GetSecretAsync(secretName);
    ///         return secret.Value.Value;
    ///     }
    ///     catch (Azure.RequestFailedException azureEx)
    ///     {
    ///         // Azure SDK exception - wrap and preserve details
    ///         throw new PvNugsStaticSecretManagerException(azureEx);
    ///     }
    ///     catch (HttpRequestException httpEx)
    ///     {
    ///         // Network exception - wrap and preserve details
    ///         throw new PvNugsStaticSecretManagerException(httpEx);
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         // Any other exception - wrap and preserve details
    ///         throw new PvNugsStaticSecretManagerException(ex);
    ///     }
    /// }
    /// </code>
    /// 
    /// <para>Exception analysis in calling code:</para>
    /// <code>
    /// try
    /// {
    ///     var secret = await secretManager.GetStaticSecretAsync("database-password");
    /// }
    /// catch (PvNugsStaticSecretManagerException ex)
    /// {
    ///     // Analyze the inner exception for specific error handling
    ///     switch (ex.InnerException)
    ///     {
    ///         case Azure.RequestFailedException { Status: 404 }:
    ///             logger.LogWarning("Secret not found in Key Vault: {SecretName}", secretName);
    ///             return null;
    ///             
    ///         case Azure.RequestFailedException { Status: 403 }:
    ///             logger.LogError(ex, "Access denied to Key Vault secret");
    ///             throw new UnauthorizedAccessException("Insufficient Key Vault permissions", ex);
    ///             
    ///         case HttpRequestException:
    ///             logger.LogError(ex, "Network error communicating with Key Vault");
    ///             throw new ServiceUnavailableException("Key Vault temporarily unavailable", ex);
    ///             
    ///         default:
    ///             logger.LogError(ex, "Unexpected error retrieving secret");
    ///             throw;
    ///     }
    /// }
    /// </code>
    /// 
    /// <para>Logging wrapped exceptions with full details:</para>
    /// <code>
    /// catch (PvNugsStaticSecretManagerException ex)
    /// {
    ///     // Log both wrapper and inner exception details
    ///     logger.LogError(ex, "Secret management operation failed");
    ///     
    ///     if (ex.InnerException is Azure.RequestFailedException azureEx)
    ///     {
    ///         logger.LogDebug("Azure error details - Status: {Status}, ErrorCode: {ErrorCode}, RequestId: {RequestId}",
    ///             azureEx.Status, azureEx.ErrorCode, azureEx.ClientRequestId);
    ///     }
    ///     
    ///     // The complete stack trace includes both wrapper and inner exception stacks
    ///     logger.LogTrace("Full exception details: {ExceptionDetails}", ex.ToString());
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="System.Exception.InnerException"/>
    /// <seealso cref="PvNugsStaticSecretManagerException(string)"/>
    /// <seealso cref="System.Exception.ToString"/>
    public PvNugsStaticSecretManagerException(Exception e) : 
        base($"PvNugsStaticSecretManagerException: {e.Message}", e)
    {
    }
}