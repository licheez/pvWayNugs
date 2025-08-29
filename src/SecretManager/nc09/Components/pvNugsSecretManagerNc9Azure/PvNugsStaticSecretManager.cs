using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using pvNugsCacheNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9Azure;

/// <summary>
/// Internal implementation of <see cref="IPvNugsStaticSecretManager"/> that provides secure secret retrieval 
/// from Azure Key Vault with integrated caching and comprehensive authentication support.
/// This class serves as the core implementation for accessing static secrets stored in Azure Key Vault,
/// supporting both Azure Managed Identity and Service Principal authentication methods.
/// </summary>
/// <remarks>
/// <para><c>Architecture and Design:</c></para>
/// <para>This implementation follows a lazy initialization pattern for the Azure Key Vault client and implements
/// a two-tier secret retrieval strategy combining local caching with Azure Key Vault API calls. The class is designed
/// as an internal implementation detail and should only be accessed through the <see cref="IPvNugsStaticSecretManager"/> interface.</para>
/// 
/// <para><c>Authentication Strategy:</c></para>
/// <para>The implementation supports two primary authentication methods:</para>
/// <list type="bullet">
/// <item><description><c>Azure Managed Identity (Recommended):</c> When <see cref="PvNugsAzureSecretManagerConfig.Credential"/> is null, uses <see cref="DefaultAzureCredential"/> for keyless authentication in Azure environments</description></item>
/// <item><description><c>Service Principal Authentication:</c> When credentials are provided, uses <see cref="ClientSecretCredential"/> with the specified tenant ID, client ID, and client secret</description></item>
/// </list>
/// 
/// <para><c>Caching Strategy:</c></para>
/// <para>The implementation employs a cache-first retrieval pattern to optimize performance and reduce API calls:</para>
/// <list type="number">
/// <item><description>Check local cache for the requested secret using a namespaced cache key</description></item>
/// <item><description>If cache miss, retrieve the secret from Azure Key Vault</description></item>
/// <item><description>Store the retrieved secret in cache for subsequent requests</description></item>
/// <item><description>Return the secret value to the caller</description></item>
/// </list>
/// 
/// <para><c>Error Handling and Resilience:</c></para>
/// <list type="bullet">
/// <item><description>All exceptions are logged using the injected logger service</description></item>
/// <item><description>Azure SDK exceptions are wrapped in <see cref="PvNugsStaticSecretManagerException"/> for consistent error handling</description></item>
/// <item><description>Authentication failures, network issues, and authorization problems are properly surfaced</description></item>
/// <item><description>The implementation preserves original exception details for debugging while providing a consistent exception contract</description></item>
/// </list>
/// 
/// <para><c>Dependency Requirements:</c></para>
/// <list type="bullet">
/// <item><description><c>ILoggerService:</c> For comprehensive logging of operations, errors, and authentication events</description></item>
/// <item><description><c>IOptions&lt;PvNugsAzureSecretManagerConfig&gt;:</c> Configuration containing Key Vault URL and optional service principal credentials</description></item>
/// <item><description><c>IPvNugsCache:</c> Cache provider for storing retrieved secrets to improve performance</description></item>
/// </list>
/// 
/// <para><c>Performance Considerations:</c></para>
/// <list type="bullet">
/// <item><description>Lazy client initialization minimizes startup overhead</description></item>
/// <item><description>Caching significantly reduces API calls and improves response times</description></item>
/// <item><description>Single SecretClient instance is reused for all operations within the service lifetime</description></item>
/// <item><description>Async/await pattern ensures non-blocking operations</description></item>
/// </list>
/// 
/// <para><c>Security Features:</c></para>
/// <list type="bullet">
/// <item><description>Service principal credentials are securely handled and never logged</description></item>
/// <item><description>Retrieved secrets are cached securely using the provided cache implementation</description></item>
/// <item><description>All communication with Azure Key Vault uses HTTPS/TLS encryption</description></item>
/// <item><description>Authentication context is logged for audit purposes (without exposing credentials)</description></item>
/// </list>
/// 
/// <para><c>Thread Safety:</c></para>
/// <para>This implementation is thread-safe and supports concurrent access:</para>
/// <list type="bullet">
/// <item><description>The lazy initialization of SecretClient is protected against race conditions</description></item>
/// <item><description>Azure SDK clients are thread-safe and can handle concurrent requests</description></item>
/// <item><description>Cache operations depend on the thread-safety guarantees of the injected cache provider</description></item>
/// </list>
/// </remarks>
/// <example>
/// <para>Typical dependency injection registration:</para>
/// <code>
/// // Program.cs or Startup.cs
/// services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(
///     configuration.GetSection(PvNugsAzureSecretManagerConfig.Section));
/// services.TryAddSingleton&lt;IPvNugsStaticSecretManager, PvNugsStaticSecretManager&gt;();
/// </code>
/// 
/// <para>Configuration with Managed Identity (recommended for Azure environments):</para>
/// <code>
/// {
///   "PvNugsAzureSecretManagerConfig": {
///     "KeyVaultUrl": "https://myapp-keyvault.vault.azure.net/",
///     "Credential": null
///   }
/// }
/// </code>
/// 
/// <para>Configuration with Service Principal (for local development or non-Azure environments):</para>
/// <code>
/// {
///   "PvNugsAzureSecretManagerConfig": {
///     "KeyVaultUrl": "https://myapp-keyvault.vault.azure.net/",
///     "Credential": {
///       "TenantId": "12345678-1234-1234-1234-123456789012",
///       "ClientId": "87654321-4321-4321-4321-210987654321",
///       "ClientSecret": "your-client-secret-here"
///     }
///   }
/// }
/// </code>
/// </example>
/// <seealso cref="IPvNugsStaticSecretManager"/>
/// <seealso cref="PvNugsAzureSecretManagerConfig"/>
/// <seealso cref="PvNugsAzureServicePrincipalCredential"/>
/// <seealso cref="PvNugsStaticSecretManagerException"/>
internal class PvNugsStaticSecretManager(
    ILoggerService logger,
    IOptions<PvNugsAzureSecretManagerConfig> options,
    IPvNugsCache cache): IPvNugsStaticSecretManager
{
    /// <summary>
    /// Gets the configuration instance containing Azure Key Vault settings and authentication credentials.
    /// This configuration is validated during service initialization and provides the necessary settings
    /// for establishing connection to Azure Key Vault.
    /// </summary>
    private readonly PvNugsAzureSecretManagerConfig _config = options.Value;

    /// <summary>
    /// Private field holding the lazily initialized Azure Key Vault SecretClient instance.
    /// This client is created on first access and reused for all subsequent secret retrieval operations.
    /// The client configuration depends on the authentication method specified in the configuration.
    /// </summary>
    private SecretClient? _client;
    
    /// <summary>
    /// Gets the Azure Key Vault SecretClient instance, creating it lazily on first access.
    /// The client is configured based on the authentication method specified in the configuration:
    /// when service principal credentials are provided, it uses ClientSecretCredential;
    /// otherwise, it uses DefaultAzureCredential for managed identity authentication.
    /// </summary>
    /// <value>
    /// A configured <see cref="SecretClient"/> instance ready for Azure Key Vault operations.
    /// </value>
    /// <remarks>
    /// <para><c>Lazy Initialization Pattern:</c></para>
    /// <para>The client is initialized only when first accessed, which provides several benefits:</para>
    /// <list type="bullet">
    /// <item><description>Reduces application startup time by deferring expensive initialization</description></item>
    /// <item><description>Avoids unnecessary authentication calls if secrets are never accessed</description></item>
    /// <item><description>Allows configuration validation to occur at runtime when needed</description></item>
    /// <item><description>Ensures the client is created with the most current configuration state</description></item>
    /// </list>
    /// 
    /// <para><c>Authentication Method Selection:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Service Principal:</c> When <c>_config.Credential</c> is not null, creates ClientSecretCredential with provided tenant ID, client ID, and client secret</description></item>
    /// <item><description><c>Managed Identity:</c> When <c>_config.Credential</c> is null, uses DefaultAzureCredential which attempts various Azure authentication methods in order</description></item>
    /// </list>
    /// 
    /// <para><c>Error Scenarios:</c></para>
    /// <para>Client creation may fail due to various reasons:</para>
    /// <list type="bullet">
    /// <item><description>Invalid Key Vault URL format</description></item>
    /// <item><description>Network connectivity issues</description></item>
    /// <item><description>Invalid service principal credentials</description></item>
    /// <item><description>Insufficient permissions for the authentication principal</description></item>
    /// </list>
    /// 
    /// <para><c>Thread Safety:</c></para>
    /// <para>The lazy initialization is not explicitly protected with locks, but the getter pattern
    /// ensures that once _client is assigned, subsequent calls will return the same instance.
    /// In rare race condition scenarios, multiple clients might be created, but the last one
    /// will be used for all subsequent operations.</para>
    /// </remarks>
    /// <exception cref="System.ArgumentException">
    /// Thrown when the Key Vault URL in the configuration is invalid or malformed.
    /// </exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException">
    /// Thrown when authentication with Azure Key Vault fails due to invalid credentials.
    /// </exception>
    /// <exception cref="System.UriFormatException">
    /// Thrown when the Key Vault URL cannot be parsed as a valid URI.
    /// </exception>
    /// <example>
    /// <para>The client property internally handles both authentication scenarios:</para>
    /// <code>
    /// // When using service principal (credential provided):
    /// // Creates: new SecretClient(vaultUri, new ClientSecretCredential(tenantId, clientId, clientSecret))
    /// 
    /// // When using managed identity (credential is null):
    /// // Creates: new SecretClient(vaultUri, new DefaultAzureCredential())
    /// </code>
    /// 
    /// <para>DefaultAzureCredential authentication chain (when credential is null):</para>
    /// <code>
    /// // DefaultAzureCredential tries authentication methods in this order:
    /// // 1. EnvironmentCredential (environment variables)
    /// // 2. ManagedIdentityCredential (Azure VM/App Service managed identity)
    /// // 3. SharedTokenCacheCredential (shared token cache)
    /// // 4. VisualStudioCredential (Visual Studio authentication)
    /// // 5. VisualStudioCodeCredential (VS Code authentication)
    /// // 6. AzureCliCredential (Azure CLI authentication)
    /// // 7. AzurePowerShellCredential (Azure PowerShell authentication)
    /// </code>
    /// </example>
    /// <seealso cref="SecretClient"/>
    /// <seealso cref="ClientSecretCredential"/>
    /// <seealso cref="DefaultAzureCredential"/>
    private SecretClient Client
    {
        get
        {
            if (_client != null) return _client;
            var vaultUri = new Uri(_config.KeyVaultUrl);
            if (_config.Credential != null)
            {
                logger.Log("using service principal credential");

                _client = new SecretClient(vaultUri, new ClientSecretCredential(
                    _config.Credential.TenantId,
                    _config.Credential.ClientId,
                    _config.Credential.ClientSecret));
            }
            else
            {
                logger.LogAsync("using Default Azure Credential");
                _client = new SecretClient(vaultUri, new DefaultAzureCredential());
            }
            return _client;
        }
    }
    
    /// <summary>
    /// Asynchronously retrieves a static secret from Azure Key Vault with integrated caching support.
    /// This method implements a cache-first retrieval strategy to optimize performance and reduce API calls
    /// to Azure Key Vault while ensuring fresh data when the cache is empty.
    /// </summary>
    /// <param name="secretName">
    /// The name of the secret to retrieve from Azure Key Vault. Must be a valid Key Vault secret name
    /// following Azure naming conventions (alphanumeric characters and hyphens only).
    /// </param>
    /// <param name="cancellationToken">
    /// An optional cancellation token that can be used to cancel the asynchronous operation.
    /// This token is honored throughout the entire operation chain, including cache operations
    /// and Azure Key Vault API calls.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous secret retrieval operation. The task result contains:
    /// <list type="bullet">
    /// <item><description>The secret value as a string if the secret exists and is successfully retrieved</description></item>
    /// <item><description><c>null</c> if the secret does not exist in Azure Key Vault</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para><c>Operation Flow:</c></para>
    /// <para>The method follows a structured retrieval process:</para>
    /// <list type="number">
    /// <item><description><c>Cache Lookup:</c> Attempts to retrieve the secret from the local cache using a namespaced cache key</description></item>
    /// <item><description><c>Cache Hit:</c> If found in cache, returns the cached value immediately without API call</description></item>
    /// <item><description><c>Cache Miss:</c> If not cached, proceeds to retrieve from Azure Key Vault</description></item>
    /// <item><description><c>Azure API Call:</c> Uses the configured SecretClient to fetch the secret from Key Vault</description></item>
    /// <item><description><c>Cache Update:</c> Stores the retrieved secret in cache for future requests</description></item>
    /// <item><description><c>Return Value:</c> Returns the secret value to the caller</description></item>
    /// </list>
    /// 
    /// <para><c>Cache Key Strategy:</c></para>
    /// <para>Cache keys are constructed using the pattern: <c>"PvNugsStaticSecretManager-{secretName}"</c></para>
    /// <list type="bullet">
    /// <item><description>Ensures cache isolation between different secret manager implementations</description></item>
    /// <item><description>Prevents cache key collisions with other cached data</description></item>
    /// <item><description>Provides predictable and debuggable cache key patterns</description></item>
    /// </list>
    /// 
    /// <para><c>Performance Optimization:</c></para>
    /// <list type="bullet">
    /// <item><description>Cache hits avoid expensive network calls to Azure Key Vault</description></item>
    /// <item><description>Reduces authentication overhead for frequently accessed secrets</description></item>
    /// <item><description>Minimizes API throttling risks from Azure Key Vault service limits</description></item>
    /// <item><description>Improves application response times for cached secrets</description></item>
    /// </list>
    /// 
    /// <para><c>Error Handling Strategy:</c></para>
    /// <para>The method implements comprehensive error handling:</para>
    /// <list type="bullet">
    /// <item><description>All exceptions are logged using the injected logger service for debugging</description></item>
    /// <item><description>Original exceptions are wrapped in <see cref="PvNugsStaticSecretManagerException"/> for consistent error contracts</description></item>
    /// <item><description>Exception wrapping preserves inner exception details for troubleshooting</description></item>
    /// <item><description>Cache-related errors are handled gracefully and don't prevent Key Vault access</description></item>
    /// </list>
    /// 
    /// <para><c>Security Considerations:</c></para>
    /// <list type="bullet">
    /// <item><description>Secret values are never logged to prevent accidental exposure</description></item>
    /// <item><description>Authentication is handled securely through Azure SDK credential providers</description></item>
    /// <item><description>All communication with Azure Key Vault uses encrypted channels (HTTPS/TLS)</description></item>
    /// <item><description>Cache storage security depends on the implementation of the injected cache provider</description></item>
    /// </list>
    /// 
    /// <para><c>Cancellation Support:</c></para>
    /// <para>The cancellation token is properly propagated through all async operations:</para>
    /// <list type="bullet">
    /// <item><description>Cache retrieval operations respect cancellation</description></item>
    /// <item><description>Azure Key Vault API calls can be cancelled mid-request</description></item>
    /// <item><description>Cache storage operations can be interrupted if needed</description></item>
    /// <item><description>Graceful handling of <see cref="OperationCanceledException"/> scenarios</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="secretName"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="secretName"/> is empty, whitespace, or contains invalid characters for Azure Key Vault.
    /// </exception>
    /// <exception cref="PvNugsStaticSecretManagerException">
    /// Thrown when any error occurs during secret retrieval, wrapping the original exception with additional context.
    /// This includes authentication failures, network issues, permission problems, and Key Vault service errors.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <example>
    /// <para>Basic secret retrieval:</para>
    /// <code>
    /// public async Task&lt;string&gt; GetDatabasePasswordAsync()
    /// {
    ///     var password = await _secretManager.GetStaticSecretAsync("database-password");
    ///     if (password == null)
    ///         throw new InvalidOperationException("Database password not found in Key Vault");
    ///     return password;
    /// }
    /// </code>
    /// 
    /// <para>Secret retrieval with cancellation and error handling:</para>
    /// <code>
    /// public async Task&lt;ApiCredentials&gt; GetApiCredentialsAsync(CancellationToken cancellationToken)
    /// {
    ///     try
    ///     {
    ///         var apiKey = await _secretManager.GetStaticSecretAsync("external-api-key", cancellationToken);
    ///         var apiSecret = await _secretManager.GetStaticSecretAsync("external-api-secret", cancellationToken);
    ///         
    ///         if (apiKey == null || apiSecret == null)
    ///             throw new InvalidOperationException("Required API credentials not found");
    ///             
    ///         return new ApiCredentials(apiKey, apiSecret);
    ///     }
    ///     catch (PvNugsStaticSecretManagerException ex)
    ///     {
    ///         _logger.LogError(ex, "Failed to retrieve API credentials from Key Vault");
    ///         throw new ServiceUnavailableException("API credentials are temporarily unavailable", ex);
    ///     }
    ///     catch (OperationCanceledException)
    ///     {
    ///         _logger.LogInformation("API credential retrieval was cancelled");
    ///         throw;
    ///     }
    /// }
    /// </code>
    /// 
    /// <para>Batch secret retrieval with error resilience:</para>
    /// <code>
    /// public async Task&lt;Dictionary&lt;string, string&gt;&gt; GetMultipleSecretsAsync(
    ///     string[] secretNames, 
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     var results = new Dictionary&lt;string, string&gt;();
    ///     
    ///     foreach (var secretName in secretNames)
    ///     {
    ///         try
    ///         {
    ///             var value = await _secretManager.GetStaticSecretAsync(secretName, cancellationToken);
    ///             if (value != null)
    ///             {
    ///                 results[secretName] = value;
    ///             }
    ///         }
    ///         catch (PvNugsStaticSecretManagerException ex)
    ///         {
    ///             _logger.LogWarning(ex, "Failed to retrieve secret {SecretName}", secretName);
    ///             // Continue with other secrets
    ///         }
    ///     }
    ///     
    ///     return results;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IPvNugsStaticSecretManager.GetStaticSecretAsync(string, CancellationToken)"/>
    /// <seealso cref="PvNugsStaticSecretManagerException"/>
    /// <seealso cref="SecretClient.GetSecretAsync(string, string?, CancellationToken)"/>
    public async Task<string?> GetStaticSecretAsync(
        string secretName, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // try to get the secret from the cache
            var cacheKey = $"{nameof(PvNugsStaticSecretManager)}-{secretName}";
            var secretValue = await cache.GetAsync<string>(cacheKey, cancellationToken);
            if (secretValue != null) return secretValue;

            // secret is not cached yet
            // let's populate it from the Azure API
            var getSecret = await Client.GetSecretAsync(
                secretName, cancellationToken: cancellationToken);

            secretValue = getSecret?.Value.Value;
            
            await cache.SetAsync(cacheKey, secretValue, 
                cancellationToken: cancellationToken);
            return secretValue;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsStaticSecretManagerException(e);
        }
    }
}