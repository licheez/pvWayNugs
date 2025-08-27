using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using pvNugsCacheNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9Azure;

internal class PvNugsStaticSecretManager(
    ILoggerService logger,
    IOptions<PvNugsAzureSecretManagerConfig> options,
    IPvNugsCache cache): IPvNugsStaticSecretManager
{
    private readonly PvNugsAzureSecretManagerConfig _config = options.Value;

    private SecretClient? _client;
    private SecretClient Client
    {
        get
        {
            if (_client != null) return _client;
            var vaultUri = new Uri(_config.KeyVaultUrl);
            if (_config.Credential != null)
            {
                logger.Log("using custom client credential");

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
            throw new PvNugsSecretManagerException(e);
        }
    }
}