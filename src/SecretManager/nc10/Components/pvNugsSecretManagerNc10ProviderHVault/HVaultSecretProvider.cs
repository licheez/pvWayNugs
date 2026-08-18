using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace pvNugsSecretManagerNc10ProviderHVault;

/// <summary>
/// HashiCorp Vault implementation of the <see cref="IPvNugsSecretProvider"/> interface.
/// Provides access to both static secrets (Key-Value store) and dynamic credentials (Database secrets engine).
/// Supports Token-based and Kubernetes authentication methods.
/// </summary>
public class HVaultSecretProvider(
    IConsoleLoggerService logger,
    IOptions<PvNugsHVaultSecretProviderConfig> options) : IPvNugsSecretProvider
{
    private readonly PvNugsHVaultSecretProviderConfig _config = options.Value;

    /// <summary>
    /// Gets the lazily-initialized Vault client instance.
    /// The client is created on first access and reused for subsequent operations.
    /// </summary>
    private VaultClient VClient
    {
        get => field ??= GetVaultClientAsync().GetAwaiter().GetResult();
    } = null;

    /// <summary>
    /// Indicates whether this provider supports dynamic database credentials.
    /// </summary>
    public bool SupportsDatabaseSecrets => true;

    /// <summary>
    /// Retrieves multiple static secrets from the HashiCorp Vault Key-Value (v2) secrets engine.
    /// </summary>
    /// <param name="parameters">
    /// A dictionary containing the required parameters:
    /// <list type="bullet">
    /// <item><see cref="PvNugsHVaultSecretProviderParameters.MountPoint"/> - The KV secrets engine mount point</item>
    /// <item><see cref="PvNugsHVaultSecretProviderParameters.Path"/> - The path to the secret within the KV store</item>
    /// </list>
    /// </param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A read-only dictionary containing all key-value pairs stored at the specified path.</returns>
    /// <exception cref="PvNugsHVaultException">Thrown when Vault operations fail or secrets cannot be retrieved.</exception>
    public async Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var kvEngine = VClient.V1.Secrets.KeyValue.V2;
        Secret<SecretData> readDictionarySecret;
        var mountPoint = parameters[PvNugsHVaultSecretProviderParameters.MountPoint];
        var path = parameters[PvNugsHVaultSecretProviderParameters.Path];
        try
        {
            readDictionarySecret = await kvEngine.ReadSecretAsync(
                path, mountPoint: mountPoint);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsHVaultException(e);
        }
        
        var dicSecretData = readDictionarySecret.Data;
        var dictionary = dicSecretData.Data.ToDictionary(
            entry => entry.Key, 
            entry => 
                entry.Value?.ToString()??string.Empty);
        return dictionary;
    }

    /// <summary>
    /// Retrieves a single static secret by name from the HashiCorp Vault Key-Value (v2) secrets engine.
    /// </summary>
    /// <param name="parameters">
    /// A dictionary containing the required parameters:
    /// <list type="bullet">
    /// <item><see cref="PvNugsHVaultSecretProviderParameters.MountPoint"/> - The KV secrets engine mount point</item>
    /// <item><see cref="PvNugsHVaultSecretProviderParameters.Path"/> - The path to the secret within the KV store</item>
    /// <item><see cref="PvNugsHVaultSecretProviderParameters.SecretName"/> - The name of the specific secret to retrieve</item>
    /// </list>
    /// </param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The secret value as a string, or null if the secret name is not found.</returns>
    /// <exception cref="PvNugsHVaultException">Thrown when Vault operations fail or secrets cannot be retrieved.</exception>
    public async Task<string?> GetStaticSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var dic = await GetStaticSecretsAsync(
            parameters, cancellationToken);
        var secretName = parameters[PvNugsHVaultSecretProviderParameters.SecretName];
        return dic.GetValueOrDefault(secretName);
    }

    /// <summary>
    /// Generates dynamic database credentials from the HashiCorp Vault Database secrets engine.
    /// The credentials are temporary and will expire after their time-to-live period.
    /// </summary>
    /// <param name="parameters">
    /// A dictionary containing the required parameters:
    /// <list type="bullet">
    /// <item><see cref="PvNugsHVaultSecretProviderParameters.MountPoint"/> - The Database secrets engine mount point</item>
    /// <item><see cref="PvNugsHVaultSecretProviderParameters.Role"/> - The database role name for credential generation</item>
    /// </list>
    /// </param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// An <see cref="IPvNugsDynamicCredential"/> containing the generated username, password, and expiration information.
    /// </returns>
    /// <exception cref="PvNugsHVaultException">Thrown when Vault operations fail or credentials cannot be generated.</exception>
    public async Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dbMountPoint = parameters[PvNugsHVaultSecretProviderParameters.MountPoint].ToLower();
            var dbRoleName = parameters[PvNugsHVaultSecretProviderParameters.Role].ToLower();
            
            var dbSecretEngine = VClient.V1.Secrets.Database;
            var dbSecret = await dbSecretEngine.GetCredentialsAsync(
                dbRoleName, dbMountPoint);

            var ttlInSeconds = dbSecret.LeaseDurationSeconds;
            var dbCredential = new HVaultDatabaseSecret(
                dbSecret.Data.Username, 
                dbSecret.Data.Password, 
                TimeSpan.FromSeconds(ttlInSeconds));
            
            return dbCredential;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsHVaultException(e);
        }
    }
    
    /// <summary>
    /// Creates and configures a Vault client based on the configured authentication method.
    /// Reads the token from file if not already provided in configuration.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A configured <see cref="VaultClient"/> instance.</returns>
    /// <exception cref="NotSupportedException">Thrown when an unsupported authentication method is configured.</exception>
    /// <exception cref="PvNugsHVaultException">Thrown when token reading fails.</exception>
    private async Task<VaultClient> GetVaultClientAsync(
        CancellationToken cancellationToken = default)
    {
        _config.Token ??= await ReadTokenAsync(_config.TokenFilePath, 
            cancellationToken);
        return _config.AuthMethod switch
        {
            PvNugsHVaultSecretProviderAuthEnu.TokenAuth => GetTokenAuthClient(),
            PvNugsHVaultSecretProviderAuthEnu.Kubernetes => GetKubeAuthClient(),
            _ => throw new 
                NotSupportedException($"Unsupported authentication method: {_config.AuthMethod}")
        };
    }
    
    /// <summary>
    /// Creates a Vault client configured with Token-based authentication.
    /// </summary>
    /// <returns>A <see cref="VaultClient"/> configured for token authentication.</returns>
    private VaultClient GetTokenAuthClient()
    {
        var auth =  new TokenAuthMethodInfo(_config.Token); 
        var vaultClientSettings = new VaultClientSettings(
            _config.ServerUrl, auth);
        return new VaultClient(vaultClientSettings);
    }
    
    /// <summary>
    /// Creates a Vault client configured with Kubernetes-based authentication.
    /// </summary>
    /// <returns>A <see cref="VaultClient"/> configured for Kubernetes authentication.</returns>
    private VaultClient GetKubeAuthClient()
    {
        var auth = new KubernetesAuthMethodInfo(
            _config.KubeMountPoint, _config.KubeRoleName, _config.Token);
        
        var vaultClientSettings = new VaultClientSettings(
            _config.ServerUrl, auth)
        {
            Namespace = _config.KubeNameSpace
        };
        return new VaultClient(vaultClientSettings);
    }
    
    /// <summary>
    /// Reads the authentication token from the specified file path.
    /// Used for loading Vault tokens or Kubernetes service account tokens from the filesystem.
    /// </summary>
    /// <param name="tokenFilePath">The absolute path to the token file.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The token content as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when tokenFilePath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the token file does not exist.</exception>
    /// <exception cref="PvNugsHVaultException">Thrown when reading the token file fails.</exception>
    private async Task<string> ReadTokenAsync(
        string tokenFilePath, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(tokenFilePath))
        {
            throw new ArgumentNullException(nameof(tokenFilePath), "Token file path cannot be null or empty.");
        }

        if (!File.Exists(tokenFilePath))
        {
            throw new FileNotFoundException($"Token file not found at path: {tokenFilePath}");
        }

        try
        {
            await logger.LogAsync(
                $"Reading token from file: {tokenFilePath}", 
                SeverityEnu.Trace);
            return await File.ReadAllTextAsync(tokenFilePath, cancellationToken);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsHVaultException(e);
        }

    }
}