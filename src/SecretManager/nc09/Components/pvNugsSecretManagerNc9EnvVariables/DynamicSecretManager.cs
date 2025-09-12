using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Provides a simulated dynamic credential management functionality by composing multiple static secrets 
/// into structured credential objects with expiration support. This implementation extends <see cref="StaticSecretManager"/> 
/// and implements <see cref="IPvNugsDynamicSecretManager"/> to retrieve username, password, and expiration date 
/// components from configuration sources as a unified credential object.
/// </summary>
/// <remarks>
/// <para><strong>⚠️ Important - Simulation vs. True Dynamic Credentials:</strong></para>
/// <para>This implementation provides a <em>simulation</em> of dynamic credentials rather than true dynamic secret 
/// management as offered by enterprise solutions like HashiCorp Vault, Azure Key Vault's dynamic secrets, or AWS Secrets Manager's rotation features.</para>
/// 
/// <para><strong>What This Implementation Provides:</strong></para>
/// <list type="bullet">
/// <item><description><strong>Configuration-Based Credentials:</strong> Reads pre-configured static credentials with expiration dates from configuration sources</description></item>
/// <item><description><strong>Structured Composition:</strong> Composes username, password, and expiration date into structured credential objects</description></item>
/// <item><description><strong>Expiration Awareness:</strong> Provides expiration-aware credential retrieval for time-sensitive scenarios</description></item>
/// <item><description><strong>Graceful Degradation:</strong> Supports graceful handling of incomplete or expired credential sets</description></item>
/// <item><description><strong>Interface Compatibility:</strong> Maintains API compatibility with true dynamic credential systems</description></item>
/// </list>
/// 
/// <para><strong>What This Implementation Does NOT Provide:</strong></para>
/// <list type="bullet">
/// <item><description><strong>On-Demand Generation:</strong> Does not generate new credentials on-demand</description></item>
/// <item><description><strong>Automatic Rotation:</strong> Does not automatically rotate or refresh credentials</description></item>
/// <item><description><strong>External Communication:</strong> Does not communicate with external secret management systems for credential generation</description></item>
/// <item><description><strong>Credential Revocation:</strong> Cannot revoke or invalidate credentials in external systems</description></item>
/// <item><description><strong>Lease Management:</strong> Does not provide lease management or automatic credential lifecycle handling</description></item>
/// </list>
/// 
/// <para><strong>Primary Use Cases:</strong></para>
/// <list type="bullet">
/// <item><description><strong>Development &amp; Testing:</strong> Provides consistent interface for credential management across different environments</description></item>
/// <item><description><strong>Migration Bridge:</strong> Serves as migration path from static to truly dynamic credential management</description></item>
/// <item><description><strong>Integration Testing:</strong> Enables testing of dynamic credential workflows without complex external dependencies</description></item>
/// <item><description><strong>Proof of Concept:</strong> Allows experimentation with dynamic credential patterns before implementing production systems</description></item>
/// <item><description><strong>Fallback Implementation:</strong> Acts as fallback when true dynamic credential systems are unavailable</description></item>
/// </list>
/// 
/// <para><strong>Configuration Structure:</strong></para>
/// <para>For a given secret name "MyService", the following configuration keys are expected:</para>
/// <list type="bullet">
/// <item><description><c>{secretName}__username</c> - The username component (required)</description></item>
/// <item><description><c>{secretName}__password</c> - The password component (required)</description></item>
/// <item><description><c>{secretName}__expirationDateUtc</c> - The UTC expiration date in ISO 8601 format (required)</description></item>
/// </list>
/// <para>All three components must be present and valid for a credential to be successfully created.</para>
/// 
/// <para><strong>Expiration Handling:</strong></para>
/// <para>The expiration date serves as metadata for consuming applications to determine credential validity, 
/// but does not trigger automatic rotation or renewal. Applications must implement their own expiration 
/// monitoring and manual credential refresh processes.</para>
/// 
/// <para><strong>Production Migration Path:</strong></para>
/// <para>For true dynamic credentials in production environments, consider migrating to:</para>
/// <list type="bullet">
/// <item><description><strong>HashiCorp Vault:</strong> Database secrets engine with automatic credential generation</description></item>
/// <item><description><strong>Azure Key Vault:</strong> Managed identity and dynamic secrets with automatic rotation</description></item>
/// <item><description><strong>AWS Secrets Manager:</strong> Automatic rotation with Lambda-based credential generation</description></item>
/// <item><description><strong>Google Secret Manager:</strong> Integration with Cloud IAM for dynamic access</description></item>
/// </list>
/// 
/// <para><strong>Thread Safety:</strong></para>
/// <para>This class inherits thread-safety characteristics from <see cref="StaticSecretManager"/> and is safe 
/// for concurrent read operations across multiple threads. Configuration reads are atomic and do not require external synchronization.</para>
/// 
/// <para><strong>Error Handling Strategy:</strong></para>
/// <para>The class implements graceful degradation where missing components result in null returns rather than exceptions, 
/// while malformed data (such as invalid date formats) throws descriptive exceptions to aid in configuration debugging.</para>
/// </remarks>
/// <param name="logger">The logger service for recording operations, warnings, and errors. Must not be null.</param>
/// <param name="options">Configuration options containing prefix and other settings for secret organization. Must not be null.</param>
/// <param name="configuration">The configuration provider to retrieve secrets from (appsettings.json, environment variables, etc.). Must not be null.</param>
/// <example>
/// <para><strong>Dependency Injection Setup:</strong></para>
/// <code>
/// // Register in dependency injection container
/// services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(config =&gt; 
/// {
///     config.Prefix = "MyApp"; // Optional prefix for organization
/// });
/// services.AddSingleton&lt;IPvNugsDynamicSecretManager, DynamicSecretManager&gt;();
/// </code>
/// 
/// <para><strong>Configuration Examples:</strong></para>
/// <code>
/// // Environment variables (with prefix "MyApp"):
/// // MyApp__DatabaseService__username=dbuser123
/// // MyApp__DatabaseService__password=SecurePass456!
/// // MyApp__DatabaseService__expirationDateUtc=2024-12-31T23:59:59Z
/// 
/// // Or in appsettings.json:
/// {
///   "MyApp": {
///     "DatabaseService__username": "dbuser123",
///     "DatabaseService__password": "SecurePass456!",
///     "DatabaseService__expirationDateUtc": "2024-12-31T23:59:59Z"
///   }
/// }
/// </code>
/// 
/// <para><strong>Basic Usage with Expiration Validation:</strong></para>
/// <code>
/// public class DatabaseService
/// {
///     private readonly IPvNugsDynamicSecretManager _secretManager;
///     private readonly ILogger _logger;
/// 
///     public async Task&lt;string&gt; GetConnectionStringAsync()
///     {
///         var credential = await _secretManager.GetDynamicSecretAsync("DatabaseService");
///         
///         if (credential == null)
///         {
///             _logger.LogError("Database credential configuration is incomplete");
///             throw new InvalidOperationException("Database credential not configured");
///         }
/// 
///         // Check if credential has expired
///         if (credential.ExpirationDateUtc &lt;= DateTime.UtcNow)
///         {
///             _logger.LogError("Database credential expired on {ExpirationDate}", 
///                 credential.ExpirationDateUtc);
///             throw new InvalidOperationException("Database credential has expired");
///         }
/// 
///         // Build connection string with valid credential
///         return $"Server=localhost;Database=MyApp;User Id={credential.Username};Password={credential.Password};";
///     }
/// }
/// </code>
/// 
/// <para><strong>Proactive Expiration Monitoring:</strong></para>
/// <code>
/// public class CredentialMonitoringService
/// {
///     private readonly IPvNugsDynamicSecretManager _secretManager;
///     private readonly ILogger _logger;
/// 
///     public async Task CheckCredentialExpirationAsync()
///     {
///         var services = new[] { "DatabaseService", "ExternalAPI", "RedisCache" };
/// 
///         foreach (var serviceName in services)
///         {
///             var credential = await _secretManager.GetDynamicSecretAsync(serviceName);
///             if (credential != null)
///             {
///                 var timeUntilExpiry = credential.ExpirationDateUtc - DateTime.UtcNow;
///                 
///                 if (timeUntilExpiry.TotalHours &lt; 24)
///                 {
///                     _logger.LogWarning("{ServiceName} credential expires in {Hours:F1} hours. " +
///                         "Update configuration before {ExpirationDate}", 
///                         serviceName, timeUntilExpiry.TotalHours, credential.ExpirationDateUtc);
///                 }
///                 else if (timeUntilExpiry.TotalDays &lt; 7)
///                 {
///                     _logger.LogInformation("{ServiceName} credential expires in {Days:F0} days", 
///                         serviceName, timeUntilExpiry.TotalDays);
///                 }
///             }
///             else
///             {
///                 _logger.LogWarning("{ServiceName} credential configuration is incomplete", serviceName);
///             }
///         }
///     }
/// }
/// </code>
/// 
/// <para><strong>Migration-Ready Implementation:</strong></para>
/// <code>
/// // This code works with simulation now and can later work with true dynamic credentials
/// public class ApiClient
/// {
///     private readonly IPvNugsDynamicSecretManager _secretManager;
/// 
///     public async Task&lt;string&gt; CallExternalApiAsync()
///     {
///         // Same interface works with both simulated and real dynamic credentials
///         var credential = await _secretManager.GetDynamicSecretAsync("ExternalAPI");
///         
///         if (credential?.ExpirationDateUtc &gt; DateTime.UtcNow)
///         {
///             using var client = new HttpClient();
///             client.DefaultRequestHeaders.Authorization = 
///                 new AuthenticationHeaderValue("Basic", 
///                     Convert.ToBase64String(Encoding.UTF8.GetBytes(
///                         $"{credential.Username}:{credential.Password}")));
///             
///             return await client.GetStringAsync("https://api.external.com/data");
///         }
///         
///         throw new UnauthorizedAccessException("API credential unavailable or expired");
///     }
/// }
/// </code>
/// </example>
internal class DynamicSecretManager(
    ILoggerService logger,
    IOptions<PvNugsSecretManagerEnvVariablesConfig> options,
    IConfiguration configuration) : 
    StaticSecretManager(logger, options, configuration), 
    IPvNugsDynamicSecretManager
{
    /// <summary>
    /// Asynchronously retrieves a simulated dynamic credential by composing username, password, and expiration date
    /// from individual configuration values. This method provides a unified interface for accessing structured 
    /// credential data while maintaining compatibility with true dynamic credential systems.
    /// </summary>
    /// <param name="secretName">
    /// The base name of the credential to retrieve. Cannot be null, empty, or consist only of whitespace.
    /// This name serves as a prefix to construct the individual component keys using the double-underscore convention:
    /// <list type="bullet">
    /// <item><description><c>{secretName}__username</c> - The database or service username</description></item>
    /// <item><description><c>{secretName}__password</c> - The corresponding password or API key</description></item>
    /// <item><description><c>{secretName}__expirationDateUtc</c> - The UTC expiration timestamp in ISO 8601 format</description></item>
    /// </list>
    /// The naming convention follows configuration system standards for hierarchical key organization.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe during the asynchronous operation. This token is passed to underlying
    /// configuration operations and can be used to implement timeouts or cooperative cancellation in scenarios
    /// where configuration retrieval might be slow (such as remote configuration providers).
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous credential retrieval operation. The task result contains:
    /// <list type="bullet">
    /// <item><description><strong>Success:</strong> An <see cref="IPvNugsDynamicCredential"/> object containing username, password, and expiration date when all three components are found and valid</description></item>
    /// <item><description><strong>Graceful Failure:</strong> <c>null</c> when any of the three required components (username, password, expiration date) is missing from the configuration</description></item>
    /// </list>
    /// <para><strong>Important:</strong> The returned credential represents a static snapshot from configuration
    /// at the time of the call. It will not automatically update when the underlying configuration changes,
    /// nor will it trigger renewal when the credential expires.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="secretName"/> is null, empty, or consists only of whitespace characters.
    /// This indicates incorrect API usage and is not logged as it represents a programming error rather than a runtime configuration issue.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when the expiration date string retrieved from configuration cannot be parsed as a valid UTC DateTime.
    /// The exception message includes the invalid date string and expected format information for debugging purposes.
    /// This exception is logged before being thrown to aid in configuration troubleshooting.
    /// </exception>
    /// <exception cref="PvNugsSecretManagerException">
    /// Thrown when errors occur during configuration section access or individual secret component retrieval.
    /// This wraps underlying configuration system exceptions such as:
    /// <list type="bullet">
    /// <item><description>Missing required configuration sections</description></item>
    /// <item><description>Configuration provider communication failures</description></item>
    /// <item><description>Access permission issues with configuration sources</description></item>
    /// </list>
    /// The original exception is logged and preserved as the inner exception for detailed error analysis.
    /// </exception>
    /// <remarks>
    /// <para><strong>Credential Composition Process:</strong></para>
    /// <para>This method implements a multi-step process to compose dynamic credentials:</para>
    /// <list type="number">
    /// <item><description><strong>Validation:</strong> Validates the <paramref name="secretName"/> parameter for null/empty values</description></item>
    /// <item><description><strong>Section Resolution:</strong> Locates the appropriate configuration section using inherited prefix logic</description></item>
    /// <item><description><strong>Component Retrieval:</strong> Retrieves all three components (username, password, expiration) using configuration key patterns</description></item>
    /// <item><description><strong>Completeness Check:</strong> Verifies that all components are non-null (returns null for incomplete credentials)</description></item>
    /// <item><description><strong>Date Parsing:</strong> Parses the expiration date string using culture-invariant, round-trip DateTime parsing</description></item>
    /// <item><description><strong>Object Creation:</strong> Creates and returns a new <see cref="DynamicCredential"/> instance</description></item>
    /// </list>
    /// 
    /// <para><strong>Graceful Degradation Strategy:</strong></para>
    /// <para>The method implements a graceful degradation strategy where:</para>
    /// <list type="bullet">
    /// <item><description><strong>Missing Components:</strong> Return null without throwing exceptions (allows applications to handle missing credentials appropriately)</description></item>
    /// <item><description><strong>Invalid Data:</strong> Throw descriptive exceptions (helps identify configuration problems during development)</description></item>
    /// <item><description><strong>System Errors:</strong> Wrap and re-throw with context (preserves error details while providing consistent exception types)</description></item>
    /// </list>
    /// 
    /// <para><strong>Date Format Requirements:</strong></para>
    /// <para>The expiration date must be provided in a format that can be parsed by .NET's DateTime.TryParse method 
    /// with InvariantCulture and RoundtripKind settings. Recommended formats:</para>
    /// <list type="bullet">
    /// <item><description><strong>ISO 8601:</strong> "2024-12-31T23:59:59Z" (preferred for UTC times)</description></item>
    /// <item><description><strong>Round-trip:</strong> "2024-12-31T23:59:59.0000000Z" (high precision)</description></item>
    /// <item><description><strong>Universal:</strong> "2024-12-31 23:59:59Z" (human-readable UTC)</description></item>
    /// </list>
    /// 
    /// <para><strong>Configuration Integration:</strong></para>
    /// <para>This method leverages the inherited configuration resolution from <see cref="StaticSecretManager"/>,
    /// including support for:</para>
    /// <list type="bullet">
    /// <item><description>Environment variable configuration with configurable prefixes</description></item>
    /// <item><description>JSON configuration file integration</description></item>
    /// <item><description>Multiple configuration provider composition</description></item>
    /// <item><description>Hierarchical configuration section organization</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>Typical Usage Pattern:</strong></para>
    /// <code>
    /// public async Task&lt;bool&gt; AuthenticateServiceAsync(string serviceName)
    /// {
    ///     try
    ///     {
    ///         var credential = await GetDynamicSecretAsync(serviceName);
    ///         
    ///         if (credential == null)
    ///         {
    ///             _logger.LogWarning("Credential configuration for {ServiceName} is incomplete", serviceName);
    ///             return false;
    ///         }
    /// 
    ///         // Validate expiration before use
    ///         if (credential.ExpirationDateUtc &lt;= DateTime.UtcNow)
    ///         {
    ///             _logger.LogError("Credential for {ServiceName} expired on {ExpirationDate}", 
    ///                 serviceName, credential.ExpirationDateUtc);
    ///             return false;
    ///         }
    /// 
    ///         // Use credential for authentication
    ///         return await PerformAuthentication(credential.Username, credential.Password);
    ///     }
    ///     catch (FormatException ex)
    ///     {
    ///         _logger.LogError(ex, "Invalid expiration date format for {ServiceName}", serviceName);
    ///         return false;
    ///     }
    ///     catch (PvNugsSecretManagerException ex)
    ///     {
    ///         _logger.LogError(ex, "Configuration error retrieving {ServiceName} credential", serviceName);
    ///         return false;
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Bulk Credential Validation:</strong></para>
    /// <code>
    /// public async Task&lt;Dictionary&lt;string, TimeSpan&gt;&gt; GetCredentialExpirationStatusAsync(string[] serviceNames)
    /// {
    ///     var results = new Dictionary&lt;string, TimeSpan&gt;();
    ///     
    ///     foreach (var serviceName in serviceNames)
    ///     {
    ///         var credential = await GetDynamicSecretAsync(serviceName);
    ///         if (credential != null)
    ///         {
    ///             var timeUntilExpiry = credential.ExpirationDateUtc - DateTime.UtcNow;
    ///             results[serviceName] = timeUntilExpiry;
    ///         }
    ///         else
    ///         {
    ///             results[serviceName] = TimeSpan.MinValue; // Indicates missing credential
    ///         }
    ///     }
    ///     
    ///     return results;
    /// }
    /// </code>
    /// 
    /// <para><strong>Configuration Examples:</strong></para>
    /// <code>
    /// // Environment Variables (with prefix "MyApp"):
    /// // MyApp__DatabasePrimary__username=db_user_2024_q1
    /// // MyApp__DatabasePrimary__password=Str0ng!P@ssw0rd2024
    /// // MyApp__DatabasePrimary__expirationDateUtc=2024-03-31T23:59:59Z
    /// 
    /// var dbCredential = await GetDynamicSecretAsync("DatabasePrimary");
    /// // Returns DynamicCredential with all three components
    /// 
    /// // Incomplete configuration (missing password):
    /// // MyApp__CacheService__username=cache_user
    /// // MyApp__CacheService__expirationDateUtc=2024-06-30T23:59:59Z
    /// // (password component missing)
    /// 
    /// var cacheCredential = await GetDynamicSecretAsync("CacheService");
    /// // Returns null due to missing password component
    /// 
    /// // Invalid date format:
    /// // MyApp__ExternalAPI__username=api_user
    /// // MyApp__ExternalAPI__password=api_secret
    /// // MyApp__ExternalAPI__expirationDateUtc=invalid-date-format
    /// 
    /// var apiCredential = await GetDynamicSecretAsync("ExternalAPI");
    /// // Throws FormatException with descriptive message
    /// </code>
    /// </example>
    public async Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        string secretName, 
        CancellationToken cancellationToken = new ())
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty", 
                nameof(secretName));

        var section = await GetSectionAsync();
        IConfigurationSection secret;
        
        try
        {
            secret = section.GetSection(secretName);
            if (!secret.Exists())
            {
                var ex = new InvalidOperationException($"Required configuration section '{secretName}' does not exist.");
                await Logger.LogAsync(ex);
                throw ex;
            }
        }
        catch (Exception  e)
        {
            await Logger.LogAsync(e);
            throw new PvNugsSecretManagerException(e);
        }
        var username = secret["username"];
        var password = secret["password"];
        var dateStr = secret["expirationDateUtc"];

        if (username is null || password is null || dateStr is null)
            return null;

        var dateOk = DateTime.TryParse(
            dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expDate);
        if (dateOk) 
            return new DynamicCredential(username, password, expDate);

        var err = $"Invalid date format for expiration date: '{dateStr}'. Expected UTC format.";
        await Logger.LogAsync(err);
        throw new FormatException(err);
    }
}