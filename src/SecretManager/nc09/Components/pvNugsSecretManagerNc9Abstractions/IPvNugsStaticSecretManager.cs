namespace pvNugsSecretManagerNc9Abstractions;

/// <summary>
/// Defines a contract for retrieving static secrets from secure storage systems.
/// This interface provides a standardized way to access passwords, API keys, connection strings, 
/// and other sensitive configuration data stored in external secret management systems such as 
/// Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault.
/// </summary>
/// <remarks>
/// <para><c>Purpose and Scope:</c></para>
/// <para>This interface is designed for scenarios where secrets are stored as static values in secret management systems.
/// Unlike dynamic secrets that are generated on-demand with expiration times, static secrets are persistent values
/// that remain unchanged until manually updated in the secret store.</para>
/// 
/// <para><c>Implementation Requirements:</c></para>
/// <list type="bullet">
/// <item><description>Implementations must be thread-safe to support concurrent access across multiple application threads.</description></item>
/// <item><description>Implementations should implement appropriate caching strategies to minimize API calls to external secret stores.</description></item>
/// <item><description>Implementations must handle authentication to the secret management system securely.</description></item>
/// <item><description>Implementations should provide proper error handling and logging for secret retrieval failures.</description></item>
/// <item><description>Implementations should respect cancellation tokens for graceful shutdown scenarios.</description></item>
/// </list>
/// 
/// <para><c>Security Considerations:</c></para>
/// <list type="bullet">
/// <item><description>Retrieved secrets should never be logged or exposed in error messages.</description></item>
/// <item><description>Implementations should use secure communication channels (HTTPS, TLS) when accessing external secret stores.</description></item>
/// <item><description>Authentication credentials for the secret store should be managed securely (managed identities, service accounts).</description></item>
/// <item><description>Consider implementing secret rotation policies in conjunction with this interface.</description></item>
/// </list>
/// 
/// <para><c>Common Use Cases:</c></para>
/// <list type="bullet">
/// <item><description>Database password retrieval for connection string construction</description></item>
/// <item><description>API key management for external service integrations</description></item>
/// <item><description>Certificate and encryption key retrieval</description></item>
/// <item><description>Third-party service credentials and tokens</description></item>
/// <item><description>Configuration values that contain sensitive information</description></item>
/// </list>
/// 
/// <para><c>Integration Patterns:</c></para>
/// <para>This interface is commonly used with dependency injection containers and can be registered as a singleton
/// service for application-wide secret access. It integrates well with the Options pattern and configuration systems
/// to provide secure alternatives to storing sensitive data in configuration files.</para>
/// </remarks>
/// <example>
/// <para>Basic usage with dependency injection:</para>
/// <code>
/// public class DatabaseService
/// {
///     private readonly IPvNugsStaticSecretManager _secretManager;
///     
///     public DatabaseService(IPvNugsStaticSecretManager secretManager)
///     {
///         _secretManager = secretManager;
///     }
///     
///     public async Task&lt;string&gt; GetConnectionStringAsync()
///     {
///         var password = await _secretManager.GetStaticSecretAsync("database-password");
///         return $"Server=myserver;Database=mydb;User=myuser;Password={password};";
///     }
/// }
/// </code>
/// 
/// <para>Usage with error handling and cancellation:</para>
/// <code>
/// public async Task&lt;ApiClient&gt; CreateApiClientAsync(CancellationToken cancellationToken)
/// {
///     try
///     {
///         var apiKey = await _secretManager.GetStaticSecretAsync("external-api-key", cancellationToken);
///         if (apiKey == null)
///         {
///             throw new InvalidOperationException("API key not found in secret store");
///         }
///         
///         return new ApiClient(apiKey);
///     }
///     catch (OperationCanceledException)
///     {
///         _logger.LogInformation("Secret retrieval was cancelled");
///         throw;
///     }
///     catch (Exception ex)
///     {
///         _logger.LogError(ex, "Failed to retrieve API key from secret manager");
///         throw;
///     }
/// }
/// </code>
/// 
/// <para>Dependency injection registration example:</para>
/// <code>
/// // Register in Program.cs or Startup.cs
/// services.AddSingleton&lt;IPvNugsStaticSecretManager, AzureKeyVaultSecretManager&gt;();
/// 
/// // Usage in configuration
/// services.Configure&lt;DatabaseOptions&gt;(options =&gt;
/// {
///     var secretManager = serviceProvider.GetRequiredService&lt;IPvNugsStaticSecretManager&gt;();
///     options.Password = await secretManager.GetStaticSecretAsync("db-password");
/// });
/// </code>
/// </example>
/// <seealso cref="IPvNugsDynamicSecretManager"/>
public interface IPvNugsStaticSecretManager
{
    /// <summary>
    /// Asynchronously retrieves a static secret value from the configured secret management system.
    /// This method provides secure access to sensitive configuration data such as passwords, API keys, 
    /// connection strings, and other credentials stored in external secret stores.
    /// </summary>
    /// <param name="secretName">
    /// The unique identifier or name of the secret to retrieve from the secret store. 
    /// This name must match the secret identifier configured in the external secret management system.
    /// The naming convention may vary depending on the secret store implementation (e.g., Azure Key Vault uses alphanumeric names with hyphens).
    /// </param>
    /// <param name="cancellationToken">
    /// An optional cancellation token that can be used to cancel the asynchronous secret retrieval operation.
    /// This is particularly useful for graceful application shutdown scenarios or when implementing timeouts.
    /// Defaults to <see cref="CancellationToken.None"/> if not specified.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous secret retrieval operation. The task result contains:
    /// <list type="bullet">
    /// <item><description>The secret value as a string if the secret exists and is successfully retrieved</description></item>
    /// <item><description><c>null</c> if the secret does not exist in the secret store</description></item>
    /// </list>
    /// The returned secret value should be treated as highly sensitive and must not be logged or exposed in error messages.
    /// </returns>
    /// <remarks>
    /// <para><c>Behavior and Error Handling:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Success:</c> Returns the secret value when found and successfully retrieved.</description></item>
    /// <item><description><c>Not Found:</c> Returns <c>null</c> when the specified secret does not exist in the secret store.</description></item>
    /// <item><description><c>Access Denied:</c> Throws an appropriate exception when authentication or authorization fails.</description></item>
    /// <item><description><c>Network Issues:</c> Throws an exception for connectivity problems with the secret store.</description></item>
    /// <item><description><c>Cancellation:</c> Throws <see cref="OperationCanceledException"/> when the operation is cancelled via the cancellation token.</description></item>
    /// </list>
    /// 
    /// <para><c>Caching Considerations:</c></para>
    /// <para>Implementations may cache retrieved secrets to improve performance and reduce API calls to external secret stores.
    /// However, caching strategies should balance performance with security requirements and should consider secret rotation policies.
    /// Cached secrets should be stored securely and cleared appropriately during application shutdown.</para>
    /// 
    /// <para><c>Thread Safety:</c></para>
    /// <para>This method must be implemented in a thread-safe manner to support concurrent access from multiple application threads.
    /// Implementations should handle concurrent requests for the same secret efficiently, potentially using techniques like 
    /// double-checked locking or semaphore-based coordination to prevent redundant API calls.</para>
    /// 
    /// <para><c>Authentication and Authorization:</c></para>
    /// <para>Implementations are responsible for authenticating with the secret management system using appropriate credentials
    /// such as managed identities, service accounts, API keys, or certificate-based authentication. The authentication mechanism
    /// should be configured during the implementation's initialization and should not require per-request credentials.</para>
    /// 
    /// <para><c>Secret Name Formats:</c></para>
    /// <para>The format and constraints of <paramref name="secretName"/> depend on the underlying secret store:</para>
    /// <list type="bullet">
    /// <item><description><c>Azure Key Vault:</c> Alphanumeric characters and hyphens only, case-insensitive</description></item>
    /// <item><description><c>AWS Secrets Manager:</c> Can include letters, numbers, and special characters like /_+=.@-</description></item>
    /// <item><description><c>HashiCorp Vault:</c> Path-based naming (e.g., "secret/data/myapp/database")</description></item>
    /// </list>
    /// 
    /// <para><c>Performance Considerations:</c></para>
    /// <para>Secret retrieval typically involves network calls to external services, which may introduce latency.
    /// Implementations should consider appropriate timeout values, retry policies, and circuit breaker patterns
    /// to handle temporary service unavailability gracefully. Caching frequently accessed secrets can significantly
    /// improve application performance.</para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="secretName"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="secretName"/> is empty, whitespace, or contains invalid characters for the target secret store.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">
    /// Thrown when the application lacks sufficient permissions to access the specified secret in the secret store.
    /// </exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException">
    /// Thrown when authentication with the secret management system fails due to invalid credentials or configuration.
    /// </exception>
    /// <exception cref="System.Net.Http.HttpRequestException">
    /// Thrown when network connectivity issues prevent communication with the secret management system.
    /// </exception>
    /// <exception cref="System.TimeoutException">
    /// Thrown when the secret retrieval operation exceeds the configured timeout period.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the secret manager is not properly configured or when the secret store is temporarily unavailable.
    /// </exception>
    /// <example>
    /// <para>Basic secret retrieval:</para>
    /// <code>
    /// public async Task&lt;string&gt; ConnectToDatabaseAsync()
    /// {
    ///     var password = await _secretManager.GetStaticSecretAsync("database-password");
    ///     if (password == null)
    ///     {
    ///         throw new InvalidOperationException("Database password not found");
    ///     }
    ///     
    ///     var connectionString = $"Server=myserver;Password={password};";
    ///     return connectionString;
    /// }
    /// </code>
    /// 
    /// <para>Secret retrieval with timeout and error handling:</para>
    /// <code>
    /// public async Task&lt;ApiCredentials&gt; GetApiCredentialsAsync()
    /// {
    ///     using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    ///     
    ///     try
    ///     {
    ///         var apiKey = await _secretManager.GetStaticSecretAsync("external-api-key", cts.Token);
    ///         var secret = await _secretManager.GetStaticSecretAsync("external-api-secret", cts.Token);
    ///         
    ///         if (apiKey == null || secret == null)
    ///         {
    ///             throw new InvalidOperationException("Required API credentials not found in secret store");
    ///         }
    ///         
    ///         return new ApiCredentials(apiKey, secret);
    ///     }
    ///     catch (OperationCanceledException)
    ///     {
    ///         _logger.LogWarning("API credential retrieval timed out after 30 seconds");
    ///         throw new TimeoutException("Secret retrieval operation timed out");
    ///     }
    /// }
    /// </code>
    /// 
    /// <para>Bulk secret retrieval with error resilience:</para>
    /// <code>
    /// public async Task&lt;Dictionary&lt;string, string&gt;&gt; GetApplicationSecretsAsync(
    ///     IEnumerable&lt;string&gt; secretNames,
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     var secrets = new Dictionary&lt;string, string&gt;();
    ///     var errors = new List&lt;string&gt;();
    ///     
    ///     foreach (var secretName in secretNames)
    ///     {
    ///         try
    ///         {
    ///             var value = await _secretManager.GetStaticSecretAsync(secretName, cancellationToken);
    ///             if (value != null)
    ///             {
    ///                 secrets[secretName] = value;
    ///             }
    ///             else
    ///             {
    ///                 errors.Add($"Secret '{secretName}' not found");
    ///             }
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             errors.Add($"Failed to retrieve secret '{secretName}': {ex.Message}");
    ///         }
    ///     }
    ///     
    ///     if (errors.Any())
    ///     {
    ///         _logger.LogWarning("Some secrets could not be retrieved: {Errors}", string.Join("; ", errors));
    ///     }
    ///     
    ///     return secrets;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IPvNugsDynamicSecretManager.GetDynamicSecretAsync(string, CancellationToken)"/>
    /// <seealso cref="CancellationToken"/>
    /// <seealso cref="OperationCanceledException"/>
    Task<string?> GetStaticSecretAsync(
        string secretName, CancellationToken cancellationToken = default);
}