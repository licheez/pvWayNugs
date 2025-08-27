
namespace pvNugsCsProviderNc9PgSql;

/// <summary>
/// Represents errors that occur during PostgreSQL connection string provider operations.
/// This exception is thrown when credential retrieval, configuration validation, or connection string generation fails.
/// </summary>
/// <remarks>
/// <para>This exception serves as a specialized wrapper for various types of failures that can occur within the connection string provider:</para>
/// <list type="bullet">
/// <item><description>Configuration validation errors (missing username, invalid settings)</description></item>
/// <item><description>Secret manager communication failures</description></item>
/// <item><description>Credential retrieval timeouts or errors</description></item>
/// <item><description>Dynamic secret expiration or renewal failures</description></item>
/// <item><description>Connection string building errors</description></item>
/// </list>
/// <para>The exception message is prefixed with "PvNugsCsProviderException:" to clearly identify the source of the error in logs and error handling.</para>
/// <para>When wrapping inner exceptions, the original exception is preserved for detailed error analysis and debugging.</para>
/// </remarks>
public class PvNugsCsProviderException : 
    Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsCsProviderException"/> class with a specified error message.
    /// Use this constructor for configuration validation errors or other scenarios where a descriptive message is sufficient.
    /// </summary>
    /// <param name="message">The message that describes the error. This message will be prefixed with "PvNugsCsProviderException:" for identification.</param>
    /// <remarks>
    /// <para>This constructor is typically used for:</para>
    /// <list type="bullet">
    /// <item><description>Configuration validation errors (e.g., missing username, invalid secret name)</description></item>
    /// <item><description>Provider state errors (e.g., secret manager not provisioned)</description></item>
    /// <item><description>Custom validation failures specific to the provider logic</description></item>
    /// </list>
    /// <para>The message is automatically prefixed to maintain consistency across all provider-related exceptions.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (string.IsNullOrEmpty(config.Username))
    /// {
    ///     throw new PvNugsCsProviderException("Username not found in configuration");
    /// }
    /// </code>
    /// </example>
    public PvNugsCsProviderException(string message):
        base($"PvNugsCsProviderException: {message}")
    {
        
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsCsProviderException"/> class that wraps an existing exception.
    /// Use this constructor when catching and re-throwing exceptions from secret managers, configuration systems, or other dependencies.
    /// </summary>
    /// <param name="e">The exception that is the cause of the current exception. The inner exception's message is used and the original exception is preserved for debugging.</param>
    /// <remarks>
    /// <para>This constructor is typically used for:</para>
    /// <list type="bullet">
    /// <item><description>Secret manager communication failures</description></item>
    /// <item><description>Network timeouts during credential retrieval</description></item>
    /// <item><description>Authentication failures with external services</description></item>
    /// <item><description>Configuration system access errors</description></item>
    /// <item><description>Any other external dependency failures</description></item>
    /// </list>
    /// <para>The inner exception is preserved to maintain the full error context and stack trace for debugging purposes.</para>
    /// <para>The message from the inner exception is automatically prefixed with "PvNugsCsProviderException:" for consistent error identification.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     var password = await secretManager.GetStaticSecretAsync(secretName, cancellationToken);
    /// }
    /// catch (Exception e)
    /// {
    ///     await logger.LogAsync(e);
    ///     throw new PvNugsCsProviderException(e);
    /// }
    /// </code>
    /// </example>
    public PvNugsCsProviderException(Exception e) : 
        base($"PvNugsCsProviderException: {e.Message}", e)
    {
    }
}