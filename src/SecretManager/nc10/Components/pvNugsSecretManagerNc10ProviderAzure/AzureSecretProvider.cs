using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10ProviderAzure;

/// <summary>
/// Azure Key Vault implementation of <see cref="IPvNugsSecretProvider"/>.
/// </summary>
/// <remarks>
/// <para>
/// This provider supports single static secret retrieval through Azure Key Vault.
/// </para>
/// <para>
/// The canonical key for single-secret lookup is <see cref="PvNugsAzureSecretProviderParameters.SecretName"/>.
/// </para>
/// <para>
/// Capability differences are explicit by design:
/// </para>
/// <list type="bullet">
/// <item><description><c>GetStaticSecretAsync</c> is supported.</description></item>
/// <item><description><c>GetStaticSecretsAsync</c> is not supported and always throws <see cref="PvNugsAzureProviderException"/>.</description></item>
/// <item><description><c>GetDynamicSecretAsync</c> is not supported and always throws <see cref="PvNugsAzureProviderException"/>.</description></item>
/// </list>
/// <para>
/// Authentication uses service principal credentials when configured; otherwise it falls back to
/// <see cref="DefaultAzureCredential"/>.
/// </para>
/// </remarks>
internal class AzureSecretProvider(
    IConsoleLoggerService logger,
    IOptions<PvNugsAzureSecretProviderConfig> options) : IPvNugsSecretProvider
{
    private readonly PvNugsAzureSecretProviderConfig _config = options.Value;

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

                var cred = new ClientSecretCredential(
                    _config.Credential.TenantId,
                    _config.Credential.ClientId,
                    _config.Credential.ClientSecret);

                _client = new SecretClient(vaultUri, cred);
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
    /// Retrieves multiple static secrets from Azure Key Vault.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup parameters. This dictionary is accepted only to satisfy the provider contract
    /// and is not used by Azure Key Vault for multi-secret retrieval.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// This method does not return a value because the operation is not supported by this provider.
    /// </returns>
    /// <exception cref="PvNugsAzureProviderException">
    /// Always thrown because Azure Key Vault does not provide mount/path dictionary retrieval semantics.
    /// </exception>
    public async Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        const string msg = "Azure secret provider does not " +
                           "support retrieving secret dictionaries";
        var e = new NotImplementedException(msg);
        await logger.LogAsync(e);
        throw new PvNugsAzureProviderException(e);
    }

    /// <summary>
    /// Retrieves a single static secret value from Azure Key Vault.
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup parameters. Requires the canonical key
    /// <see cref="PvNugsAzureSecretProviderParameters.SecretName"/> with a non-empty value.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The resolved secret value, or <see langword="null"/> when the backend response contains no value.
    /// </returns>
    /// <exception cref="PvNugsAzureProviderException">
    /// Thrown when the required secret-name key is missing or empty, configuration is invalid, authentication fails,
    /// or the Azure SDK call fails.
    /// </exception>
    public async Task<string?> GetStaticSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var paramFound = parameters.TryGetValue(
            PvNugsAzureSecretProviderParameters.SecretName, 
            out var secretName);
        if (!paramFound || string.IsNullOrWhiteSpace(secretName))
        {
            const string msg = "No secret name provided or empty secret name. " +
                               "GetStaticSecretAsync method expects " +
                               $"'{PvNugsAzureSecretProviderParameters.SecretName}' key/value pair";
            await logger.LogAsync(msg, SeverityEnu.Error);
            var e = new ArgumentException(msg, nameof(parameters)); 
            throw new PvNugsAzureProviderException(e);
        }

        try
        {
            var getSecret = await Client.GetSecretAsync(
                secretName, cancellationToken: cancellationToken);
            return getSecret?.Value.Value;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsAzureProviderException(e);
        }
    }

    /// <summary>
    /// Retrieves a dynamic secret (for example leased DB credentials).
    /// </summary>
    /// <param name="parameters">
    /// Provider-specific lookup parameters. This dictionary is accepted only to satisfy the provider contract
    /// and is not used by Azure Key Vault for dynamic secret generation.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// This method does not return a value because the operation is not supported by this provider.
    /// </returns>
    /// <exception cref="PvNugsAzureProviderException">
    /// Always thrown because Azure Key Vault does not provide Vault-style dynamic credential generation.
    /// </exception>
    public async Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        const string msg = "Azure secret provider does not " +
                           "support retrieving dynamic secrets";
        var e = new NotImplementedException(msg);
        await logger.LogAsync(e);
        throw new PvNugsAzureProviderException(e);
    }
}