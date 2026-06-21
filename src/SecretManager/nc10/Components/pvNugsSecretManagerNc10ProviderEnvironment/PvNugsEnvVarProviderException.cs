namespace pvNugsSecretManagerNc10ProviderEnvironment;

/// <summary>
/// Represents errors that occur during Environment Variable secret provider operations.
/// </summary>
/// <remarks>
/// <para>
/// This exception wraps configuration and environment variable access failures, preserving
/// the original exception as <see cref="Exception.InnerException"/> for diagnostics.
/// </para>
/// <para>
/// The Environment Variable provider retrieves secrets from configuration sources
/// (environment variables, appsettings.json, user secrets, etc.) through the
/// Microsoft.Extensions.Configuration system.
/// </para>
/// <para>Typical inner exception types include:</para>
/// <list type="bullet">
/// <item><description><c>System.InvalidOperationException</c> - Required configuration section not found</description></item>
/// <item><description><c>System.ArgumentException</c> - Invalid or missing parameter values</description></item>
/// <item><description><c>System.NotImplementedException</c> - Unsupported operations (dynamic secrets, secret dictionaries)</description></item>
/// <item><description><c>System.Security.SecurityException</c> - Access denied to configuration source</description></item>
/// <item><description><c>System.FormatException</c> - Configuration value format issues</description></item>
/// <item><description><c>System.IO.IOException</c> - File-based configuration access errors</description></item>
/// </list>
/// <para>
/// Use the inner exception to distinguish between configuration errors, missing values,
/// unsupported operations, and access violations.
/// </para>
/// </remarks>
/// <example>
/// <para><b>Typical exception handling in application code:</b></para>
/// <code>
/// try
/// {
///     var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword");
///     var secret = await secretManager.GetStaticSecretAsync(parameters);
///     return secret;
/// }
/// catch (PvNugsEnvVarProviderException ex)
/// {
///     logger.LogError(ex, "Failed to retrieve secret from configuration");
///     
///     // Check inner exception for specific error handling
///     if (ex.InnerException is InvalidOperationException)
///     {
///         logger.LogError("Configuration section not found. Check your appsettings.json or environment variables.");
///     }
///     else if (ex.InnerException is ArgumentException)
///     {
///         logger.LogError("Invalid parameter provided. Ensure secret name is not null or empty.");
///     }
///     
///     throw;
/// }
/// </code>
/// 
/// <para><b>Handling unsupported operations:</b></para>
/// <code>
/// try
/// {
///     // This will throw because EnvVar provider doesn't support dynamic secrets
///     var credential = await secretProvider.GetDynamicSecretAsync(parameters);
/// }
/// catch (PvNugsEnvVarProviderException ex) when (ex.InnerException is NotImplementedException)
/// {
///     logger.LogWarning("Environment Variable provider does not support dynamic secrets. Use Azure provider instead.");
///     // Fall back to alternative credential source
/// }
/// </code>
/// </example>
/// <seealso cref="Exception"/>
/// <seealso cref="System.Exception.InnerException"/>
/// <seealso cref="EnvVarSecretProvider"/>
/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions">Exception Best Practices</seealso>
public class PvNugsEnvVarProviderException : 
    Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsEnvVarProviderException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <remarks>
    /// This constructor is typically used for custom error messages without an underlying exception.
    /// Consider using the constructor overload that accepts an <see cref="Exception"/> parameter
    /// to preserve the full exception chain for better diagnostics.
    /// </remarks>
    public PvNugsEnvVarProviderException(string message):
        base($"PvNugsEnvVarProviderException: {message}")
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsEnvVarProviderException"/> class
    /// with the specified inner exception preserved as the root cause.
    /// </summary>
    /// <param name="e">
    /// The underlying exception that caused the provider operation to fail.
    /// This is typically a configuration-related exception from the Microsoft.Extensions.Configuration system.
    /// </param>
    /// <remarks>
    /// <para>
    /// This is the preferred constructor for wrapping exceptions that occur during secret retrieval,
    /// as it preserves the complete exception stack trace and details for troubleshooting.
    /// </para>
    /// <para>
    /// The provider automatically wraps common exceptions including:
    /// <list type="bullet">
    /// <item><description>Configuration access errors</description></item>
    /// <item><description>Missing section or key errors</description></item>
    /// <item><description>Parameter validation failures</description></item>
    /// <item><description>Unsupported operation attempts</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public PvNugsEnvVarProviderException(Exception e) : 
        base($"PvNugsEnvVarProviderException: {e.Message}", e)
    {
    }
}