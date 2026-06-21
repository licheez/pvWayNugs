namespace pvNugsSecretManagerNc10ProviderAzure;

/// <summary>
/// Represents errors that occur during Azure Key Vault secret management operations.
/// </summary>
/// <remarks>
/// <para>
/// This exception wraps Azure-specific failures and preserves the original exception as
/// <see cref="Exception.InnerException"/> for diagnostics.
/// </para>
/// <para>Typical inner exception types include:</para>
/// <list type="bullet">
/// <item><description><c>Azure.RequestFailedException</c></description></item>
/// <item><description><c>System.Net.Http.HttpRequestException</c></description></item>
/// <item><description><c>System.Security.Authentication.AuthenticationException</c></description></item>
/// <item><description><c>System.UnauthorizedAccessException</c></description></item>
/// <item><description><c>System.TimeoutException</c></description></item>
/// <item><description><c>System.ArgumentException</c></description></item>
/// <item><description><c>System.UriFormatException</c></description></item>
/// </list>
/// <para>Use the inner exception to distinguish service errors, authorization failures, and validation issues.</para>
/// </remarks>
/// <example>
/// <para>Typical exception handling in application code:</para>
/// <code>
/// try
/// {
///     var secret = await secretManager.GetStaticSecretAsync(parameters);
///     return secret;
/// }
/// catch (PvNugsAzureProviderException ex)
/// {
///     logger.LogError(ex, "Failed to retrieve secret from Azure Key Vault");
///     throw;
/// }
/// </code>
/// </example>
/// <seealso cref="Exception"/>
/// <seealso cref="System.Exception.InnerException"/>
/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions">Exception Best Practices</seealso>
public class PvNugsAzureProviderException : 
    Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsAzureProviderException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PvNugsAzureProviderException(string message):
        base($"PvNugsAzureProviderException: {message}")
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsAzureProviderException"/> class
    /// with the specified inner exception preserved as the root cause.
    /// </summary>
    /// <param name="e">The underlying exception.</param>
    public PvNugsAzureProviderException(Exception e) : 
        base($"PvNugsAzureProviderException: {e.Message}", e)
    {
    }
}