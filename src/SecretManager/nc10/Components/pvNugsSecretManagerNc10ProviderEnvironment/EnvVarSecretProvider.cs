using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10ProviderEnvironment;

/// <summary>
/// Provides secret retrieval from environment variables and configuration sources through
/// the Microsoft.Extensions.Configuration system.
/// </summary>
/// <remarks>
/// <para>
/// This provider retrieves secrets from any configuration source supported by 
/// Microsoft.Extensions.Configuration, including:
/// </para>
/// <list type="bullet">
/// <item><description><b>Environment variables</b> - System and user-level variables</description></item>
/// <item><description><b>JSON files</b> - appsettings.json and environment-specific files</description></item>
/// <item><description><b>User secrets</b> - Development-time secret storage</description></item>
/// <item><description><b>Command-line arguments</b> - Runtime configuration</description></item>
/// <item><description><b>In-memory collections</b> - Programmatic configuration</description></item>
/// <item><description><b>Custom providers</b> - Any IConfiguration implementation</description></item>
/// </list>
/// <para><b>Supported Operations:</b></para>
/// <list type="bullet">
/// <item><description>? <see cref="GetStaticSecretAsync"/> - Retrieve single secret values</description></item>
/// <item><description>? <see cref="GetStaticSecretsAsync"/> - Not supported (throws <see cref="NotImplementedException"/>)</description></item>
/// <item><description>? <see cref="GetDynamicSecretAsync"/> - Not supported (throws <see cref="NotImplementedException"/>)</description></item>
/// </list>
/// <para><b>Configuration Organization:</b></para>
/// <para>
/// The provider supports an optional prefix (configured in <see cref="PvNugsEnvVarSecretProviderConfig"/>)
/// to organize secrets into logical namespaces. Without a prefix, secrets are accessed from the 
/// root configuration level.
/// </para>
/// <para><b>Thread Safety:</b></para>
/// <para>
/// This provider is thread-safe for concurrent read operations. The underlying IConfiguration
/// is typically registered as a singleton and is safe for concurrent access.
/// </para>
/// </remarks>
/// <example>
/// <para><b>Registration and configuration:</b></para>
/// <code>
/// // Program.cs
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // Register the provider
/// builder.Services.TryAddPvNugsEnvVarSecretProvider(builder.Configuration);
/// 
/// // appsettings.json
/// {
///   "PvNugsEnvVarSecretProviderConfig": {
///     "Prefix": "MyApp"
///   },
///   "MyApp": {
///     "DatabasePassword": "dev_password_123",
///     "ApiKey": "dev_api_key_456"
///   }
/// }
/// </code>
/// 
/// <para><b>Retrieving secrets:</b></para>
/// <code>
/// public class MyService
/// {
///     private readonly IPvNugsSecretProvider _provider;
///     
///     public MyService(IPvNugsSecretProvider provider)
///     {
///         _provider = provider;
///     }
///     
///     public async Task&lt;string&gt; GetDatabasePassword()
///     {
///         var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword");
///         var password = await _provider.GetStaticSecretAsync(parameters);
///         return password!;
///     }
/// }
/// </code>
/// 
/// <para><b>Environment variable configuration (without prefix):</b></para>
/// <code>
/// // Set environment variable:
/// // DatabasePassword=mySecretPassword123
/// 
/// // Configuration:
/// {
///   "PvNugsEnvVarSecretProviderConfig": {
///     "Prefix": null
///   }
/// }
/// 
/// // Retrieval:
/// var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword");
/// var password = await provider.GetStaticSecretAsync(parameters);
/// // Returns: "mySecretPassword123"
/// </code>
/// 
/// <para><b>Environment variable configuration (with prefix "MyApp"):</b></para>
/// <code>
/// // Set environment variable:
/// // MyApp__DatabasePassword=mySecretPassword123
/// 
/// // Configuration:
/// {
///   "PvNugsEnvVarSecretProviderConfig": {
///     "Prefix": "MyApp"
///   }
/// }
/// 
/// // Retrieval:
/// var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword");
/// var password = await provider.GetStaticSecretAsync(parameters);
/// // Returns: "mySecretPassword123"
/// </code>
/// </example>
internal class EnvVarSecretProvider(
    IConsoleLoggerService logger,
    IOptions<PvNugsEnvVarSecretProviderConfig> options,
    IConfiguration configuration): IPvNugsSecretProvider
{
    private readonly string? _prefix = options.Value.Prefix;

    /// <summary>
    /// This operation is not supported by the Environment Variable provider.
    /// </summary>
    /// <param name="parameters">The parameters (not used).</param>
    /// <param name="cancellationToken">The cancellation token (not used).</param>
    /// <returns>This method always throws an exception.</returns>
    /// <exception cref="PvNugsEnvVarProviderException">
    /// Always thrown with a <see cref="NotImplementedException"/> inner exception.
    /// The Environment Variable provider only supports single secret retrieval via
    /// <see cref="GetStaticSecretAsync"/>.
    /// </exception>
    /// <remarks>
    /// The Environment Variable provider is designed for simple key-value secret retrieval
    /// and does not support retrieving multiple secrets in a single call. Use
    /// <see cref="GetStaticSecretAsync"/> multiple times if you need multiple secrets.
    /// </remarks>
    public async Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        const string msg = "EnvVar secret provider does not " +
                           "support retrieving secret dictionaries";
        var e = new NotImplementedException(msg);
        await logger.LogAsync(e);
        throw new PvNugsEnvVarProviderException(e);
    }

    /// <summary>
    /// Retrieves a single secret value from environment variables or configuration sources.
    /// </summary>
    /// <param name="parameters">
    /// A dictionary containing the required <see cref="PvNugsEnvVarSecretProviderParameters.SecretName"/>
    /// parameter with the configuration key name to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token (not currently used by this provider).
    /// </param>
    /// <returns>
    /// The secret value as a string, or <see langword="null"/> if the configuration key is not found.
    /// </returns>
    /// <exception cref="PvNugsEnvVarProviderException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description>The <paramref name="parameters"/> dictionary is missing the required <c>secretName</c> key</description></item>
    /// <item><description>The secret name value is null or whitespace</description></item>
    /// <item><description>The configured prefix section does not exist in configuration</description></item>
    /// <item><description>Any other configuration access error occurs</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// <para><b>Configuration Resolution:</b></para>
    /// <list type="bullet">
    /// <item><description><b>Without prefix:</b> Retrieves from root configuration level</description></item>
    /// <item><description><b>With prefix:</b> Retrieves from the specified configuration section</description></item>
    /// </list>
    /// <para>
    /// The method uses the standard Microsoft.Extensions.Configuration indexer, which means
    /// it follows configuration precedence rules (environment variables override JSON files, etc.).
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><b>Basic usage:</b></para>
    /// <code>
    /// var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("ApiKey");
    /// var apiKey = await provider.GetStaticSecretAsync(parameters);
    /// if (apiKey != null)
    /// {
    ///     Console.WriteLine($"API Key retrieved: {apiKey}");
    /// }
    /// </code>
    /// 
    /// <para><b>With error handling:</b></para>
    /// <code>
    /// try
    /// {
    ///     var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword");
    ///     var password = await provider.GetStaticSecretAsync(parameters);
    ///     
    ///     if (password == null)
    ///     {
    ///         throw new InvalidOperationException("Database password not configured");
    ///     }
    ///     
    ///     return password;
    /// }
    /// catch (PvNugsEnvVarProviderException ex)
    /// {
    ///     logger.LogError(ex, "Failed to retrieve database password");
    ///     throw;
    /// }
    /// </code>
    /// </example>
    public async Task<string?> GetStaticSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var paramFound = parameters.TryGetValue(
            PvNugsEnvVarSecretProviderParameters.SecretName, 
            out var secretName);
        if (!paramFound || string.IsNullOrWhiteSpace(secretName))
        {
            const string msg = "No secret name provided or empty secret name. " +
                               "GetStaticSecretAsync method expects " +
                               $"'{PvNugsEnvVarSecretProviderParameters.SecretName}' key/value pair";
            await logger.LogAsync(msg, SeverityEnu.Error);
            var e = new ArgumentException(msg, nameof(parameters)); 
            throw new PvNugsEnvVarProviderException(e);
        }
        
        try
        {
            var section = GetSection();
            var secret = section[secretName];
            return secret;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsEnvVarProviderException(e);
        }
    }

    /// <summary>
    /// This operation is not supported by the Environment Variable provider.
    /// </summary>
    /// <param name="parameters">The parameters (not used).</param>
    /// <param name="cancellationToken">The cancellation token (not used).</param>
    /// <returns>This method always throws an exception.</returns>
    /// <exception cref="PvNugsEnvVarProviderException">
    /// Always thrown with a <see cref="NotImplementedException"/> inner exception.
    /// The Environment Variable provider does not support dynamic credentials with expiration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Dynamic secrets (rotating credentials with expiration dates) are not supported
    /// because environment variables and standard configuration sources do not have
    /// built-in support for credential rotation or expiration tracking.
    /// </para>
    /// <para>
    /// For dynamic secrets, use a provider that supports credential rotation such as:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>AzureSecretProvider</c> - Supports Azure Key Vault managed secrets</description></item>
    /// <item><description><c>AwsSecretProvider</c> - Supports AWS Secrets Manager rotation</description></item>
    /// <item><description>Custom provider implementations with rotation support</description></item>
    /// </list>
    /// </remarks>
    public async Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        const string msg = "EnvVar secret provider does not " +
                           "support retrieving dynamic secrets";
        var e = new NotImplementedException(msg);
        await logger.LogAsync(e);
        throw new PvNugsEnvVarProviderException(e);
    }

    /// <summary>
    /// Gets the appropriate configuration section based on the configured prefix.
    /// </summary>
    /// <returns>
    /// The root <see cref="IConfiguration"/> if no prefix is configured, or the
    /// configuration section specified by the prefix.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a prefix is configured but the corresponding configuration section does not exist.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method provides the configuration scope from which secrets will be retrieved:
    /// </para>
    /// <list type="bullet">
    /// <item><description><b>No prefix (null/empty):</b> Returns the root configuration, allowing direct access to top-level keys</description></item>
    /// <item><description><b>With prefix:</b> Returns the required configuration section, organizing secrets under a namespace</description></item>
    /// </list>
    /// <para>
    /// If a prefix is specified but the section doesn't exist, an exception is thrown
    /// to fail fast and indicate misconfiguration rather than silently returning null values.
    /// </para>
    /// </remarks>
    private IConfiguration GetSection()
    {
        if (string.IsNullOrEmpty(_prefix)) return configuration;
        
        var section = configuration.GetRequiredSection(_prefix);
        if (section.Exists()) return section;
        
        var ex = new InvalidOperationException(
            $"Required configuration section '{_prefix}' does not exist.");
        logger.Log(ex);
        throw ex;
    }
}