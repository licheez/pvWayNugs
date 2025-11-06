namespace pvNugsSecretManagerNc6Abstractions;

/// <summary>
/// Extends <see cref="IPvNugsStaticSecretManager"/> to provide dynamic credential generation with automatic expiration and renewal capabilities.
/// This interface is designed for high-security environments where temporary, time-limited database credentials are required,
/// implementing zero-trust security principles and credential rotation best practices.
/// </summary>
/// <remarks>
/// <para><c>Key Differences from Static Secret Management:</c></para>
/// <para>Unlike <see cref="IPvNugsStaticSecretManager"/> which retrieves persistent secrets, this interface generates temporary credentials
/// that automatically expire after a predefined period. This approach provides enhanced security through:</para>
/// <list type="bullet">
/// <item><description>Time-limited credential validity reducing exposure risk</description></item>
/// <item><description>Automatic credential rotation without manual intervention</description></item>
/// <item><description>Dynamic username and password generation for each request or session</description></item>
/// <item><description>Elimination of long-lived credentials in configuration or secret stores</description></item>
/// </list>
/// 
/// <para><c>Implementation Requirements:</c></para>
/// <para>In addition to the requirements inherited from <see cref="IPvNugsStaticSecretManager"/>, implementations must:</para>
/// <list type="bullet">
/// <item><description>Handle credential lifecycle management including expiration tracking and renewal</description></item>
/// <item><description>Implement credential caching with expiration-aware eviction policies</description></item>
/// <item><description>Provide thread-safe access to credential renewal operations</description></item>
/// <item><description>Handle concurrent requests during credential refresh periods gracefully</description></item>
/// </list>
/// 
/// <para><c>Security Benefits:</c></para>
/// <list type="bullet">
/// <item><description>Reduced blast radius in case of credential compromise</description></item>
/// <item><description>Automatic credential rotation eliminates manual processes</description></item>
/// <item><description>No persistent credentials stored in configuration files</description></item>
/// <item><description>Compliance with zero-trust security architectures</description></item>
/// </list>
/// 
/// <para><c>Use Cases:</c></para>
/// <list type="bullet">
/// <item><description>Production database access with temporary credentials</description></item>
/// <item><description>Multi-tenant applications requiring credential isolation</description></item>
/// <item><description>Compliance environments requiring credential rotation</description></item>
/// <item><description>Cloud-native applications using managed database services</description></item>
/// </list>
/// </remarks>
/// <example>
/// <para>Basic usage for database connection:</para>
/// <code>
/// public class SecureDataService
/// {
///     private readonly IPvNugsDynamicSecretManager _secretManager;
///     
///     public SecureDataService(IPvNugsDynamicSecretManager secretManager)
///     {
///         _secretManager = secretManager;
///     }
///     
///     public async Task&lt;List&lt;User&gt;&gt; GetUsersAsync()
///     {
///         var credential = await _secretManager.GetDynamicSecretAsync("app-database");
///         if (credential == null)
///             throw new InvalidOperationException("Failed to obtain dynamic credentials");
///             
///         var connectionString = $"Server=myserver;Database=mydb;Username={credential.Username};Password={credential.Password};";
///         
///         // Use connection before expiration
///         if (DateTime.UtcNow > credential.ExpirationDateUtc)
///         {
///             throw new InvalidOperationException("Credential has expired");
///         }
///         
///         // Database operations...
///         return users;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IPvNugsStaticSecretManager"/>
/// <seealso cref="IPvNugsDynamicCredential"/>
public interface IPvNugsDynamicSecretManager : IPvNugsStaticSecretManager
{
    /// <summary>
    /// Asynchronously generates or retrieves dynamic credentials with automatic expiration for the specified secret name.
    /// This method provides temporary database credentials that are automatically rotated, offering enhanced security
    /// compared to the static secret retrieval provided by <see cref="IPvNugsStaticSecretManager.GetStaticSecretAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="secretName">
    /// The unique identifier for the dynamic secret configuration in the secret management system.
    /// This name identifies the database or service for which dynamic credentials should be generated.
    /// Unlike static secrets, this typically refers to a credential generation policy rather than a stored value.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional cancellation token for the asynchronous credential generation operation.
    /// Particularly important for dynamic credentials as the generation process may involve multiple API calls
    /// to the secret management system and the target database service.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous credential generation operation. The task result contains:
    /// <list type="bullet">
    /// <item><description>An <see cref="IPvNugsDynamicCredential"/> instance with username, password, and expiration time if successful</description></item>
    /// <item><description><c>null</c> if the dynamic secret configuration does not exist or credential generation fails</description></item>
    /// </list>
    /// The returned credentials are temporary and will automatically expire at the time specified in <see cref="IPvNugsDynamicCredential.ExpirationDateUtc"/>.
    /// </returns>
    /// <remarks>
    /// <para><c>Credential Lifecycle Management:</c></para>
    /// <para>This method handles the complete lifecycle of dynamic credentials:</para>
    /// <list type="number">
    /// <item><description><c>Generation:</c> Creates new username/password combinations through the secret management system</description></item>
    /// <item><description><c>Caching:</c> Stores credentials temporarily to avoid redundant generation calls</description></item>
    /// <item><description><c>Expiration Tracking:</c> Monitors credential validity and triggers renewal before expiration</description></item>
    /// <item><description><c>Automatic Renewal:</c> Generates fresh credentials when existing ones near expiration</description></item>
    /// </list>
    /// 
    /// <para><c>Caching Strategy:</c></para>
    /// <para>Unlike static secrets which may be cached indefinitely, dynamic credentials require expiration-aware caching:</para>
    /// <list type="bullet">
    /// <item><description>Credentials are cached until a configurable time before expiration (e.g., 5 minutes before)</description></item>
    /// <item><description>Background renewal processes may pre-generate credentials to avoid blocking operations</description></item>
    /// <item><description>Expired credentials are immediately evicted from cache and new ones generated</description></item>
    /// <item><description>Cache misses trigger immediate credential generation</description></item>
    /// </list>
    /// 
    /// <para><c>Concurrency and Thread Safety:</c></para>
    /// <para>Multiple concurrent requests for the same secret name should be handled efficiently:</para>
    /// <list type="bullet">
    /// <item><description>Only one credential generation operation should occur per secret name at a time</description></item>
    /// <item><description>Concurrent requests should wait for in-progress generation rather than starting duplicate operations</description></item>
    /// <item><description>Credential renewal should not block ongoing operations using existing valid credentials</description></item>
    /// </list>
    /// 
    /// <para><c>Error Handling and Resilience:</c></para>
    /// <para>Dynamic credential generation involves multiple external dependencies:</para>
    /// <list type="bullet">
    /// <item><description>Network failures during generation should be retried with exponential backoff</description></item>
    /// <item><description>Temporary service unavailability should fall back to cached credentials if still valid</description></item>
    /// <item><description>Generation timeouts should not leave the system in an inconsistent state</description></item>
    /// </list>
    /// 
    /// <para><c>Comparison with Static Secrets:</c></para>
    /// <para>Key differences from <see cref="IPvNugsStaticSecretManager.GetStaticSecretAsync(string, CancellationToken)"/>:</para>
    /// <list type="bullet">
    /// <item><description><c>Credential Source:</c> Generated dynamically vs retrieved from storage</description></item>
    /// <item><description><c>Lifetime:</c> Temporary with expiration vs persistent until manually changed</description></item>
    /// <item><description><c>Security:</c> Time-limited exposure vs permanent until rotation</description></item>
    /// <item><description><c>Complexity:</c> Requires lifecycle management vs simple retrieval</description></item>
    /// <item><description><c>Performance:</c> Generation latency vs simple cache/retrieve operations</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="secretName"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="secretName"/> is empty, whitespace, or invalid for the target secret management system.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the dynamic secret configuration is not found or the credential generation service is unavailable.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">
    /// Thrown when insufficient permissions exist to generate credentials for the specified secret name.
    /// </exception>
    /// <exception cref="System.TimeoutException">
    /// Thrown when credential generation exceeds the configured timeout period.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <example>
    /// <para>Basic dynamic credential usage:</para>
    /// <code>
    /// public async Task&lt;string&gt; ExecuteQueryAsync(string query)
    /// {
    ///     var credential = await _dynamicSecretManager.GetDynamicSecretAsync("production-database");
    ///     
    ///     if (credential == null)
    ///         throw new InvalidOperationException("Unable to obtain database credentials");
    ///         
    ///     // Check if credential is still valid
    ///     if (DateTime.UtcNow >= credential.ExpirationDateUtc)
    ///         throw new InvalidOperationException("Received expired credential");
    ///         
    ///     var connectionString = BuildConnectionString(credential.Username, credential.Password);
    ///     // Execute query with temporary credentials...
    /// }
    /// </code>
    /// 
    /// <para>Advanced usage with expiration handling:</para>
    /// <code>
    /// public async Task&lt;T&gt; ExecuteWithDynamicCredentialsAsync&lt;T&gt;(
    ///     string secretName,
    ///     Func&lt;string, Task&lt;T&gt;&gt; operation,
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     const int maxRetries = 3;
    ///     
    ///     for (int attempt = 0; attempt &lt; maxRetries; attempt++)
    ///     {
    ///         var credential = await _dynamicSecretManager.GetDynamicSecretAsync(secretName, cancellationToken);
    ///         
    ///         if (credential == null)
    ///             throw new InvalidOperationException($"Failed to obtain credentials for {secretName}");
    ///             
    ///         // Allow some buffer time before expiration
    ///         var bufferTime = TimeSpan.FromMinutes(2);
    ///         if (DateTime.UtcNow.Add(bufferTime) >= credential.ExpirationDateUtc)
    ///         {
    ///             _logger.LogWarning("Credential expires soon, requesting fresh credentials");
    ///             continue; // Try to get fresher credentials
    ///         }
    ///         
    ///         try
    ///         {
    ///             var connectionString = BuildConnectionString(credential.Username, credential.Password);
    ///             return await operation(connectionString);
    ///         }
    ///         catch (AuthenticationException) when (attempt &lt; maxRetries - 1)
    ///         {
    ///             _logger.LogWarning("Authentication failed, credential may have expired. Retrying...");
    ///             // Clear any cached credentials and retry
    ///             await Task.Delay(1000, cancellationToken); // Brief delay before retry
    ///         }
    ///     }
    ///     
    ///     throw new InvalidOperationException("Failed to execute operation with dynamic credentials after all retries");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IPvNugsStaticSecretManager.GetStaticSecretAsync(string, CancellationToken)"/>
    /// <seealso cref="IPvNugsDynamicCredential"/>
    /// <seealso cref="IPvNugsDynamicCredential.ExpirationDateUtc"/>
    Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        string secretName, CancellationToken cancellationToken = default);
}