using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Extensions.Options;
using pvNugsCacheNc10Abstractions;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10;

internal class SecretManager(
    IConsoleLoggerService logger,
    IPvNugsCache cache,
    IPvNugsSecretProvider provider,
    IOptions<PvNugsSecretManagerConfig> options) : IPvNugsSecretManager
{
    private readonly PvNugsSecretManagerConfig _config = options.Value;
    
    public async Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(
        IReadOnlyDictionary<string, string> parameters, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(parameters);
        await logger.LogAsync(
            $"Getting static secrets with parameters: {cacheKey}", 
            SeverityEnu.Trace);
        
        var cached = await cache.GetAsync<ReadOnlyDictionary<string, string>>(
            cacheKey, cancellationToken);
        if (cached != null) return cached;

        try
        {
            var res = await provider.GetStaticSecretsAsync(
                parameters, cancellationToken);
            await cache.SetAsync(cacheKey, res, _config.CacheTimeToLive, cancellationToken);
            return res;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e, SeverityEnu.Error);
            throw new PvNugsSecretManagerException(e);
        }
    }

    public async Task<string?> GetStaticSecretAsync(
        IReadOnlyDictionary<string, string> parameters, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(parameters);
        await logger.LogAsync(
            $"Getting static secrets with parameters: {cacheKey}",
            SeverityEnu.Trace);
        var cached = await cache.GetAsync<string>(
            cacheKey, cancellationToken);
        if (cached != null) return cached;

        try
        {
            var res = await provider.GetStaticSecretAsync(
                parameters, cancellationToken);
            if (string.IsNullOrEmpty(res)) return null;
            await cache.SetAsync(cacheKey, res, _config.CacheTimeToLive, cancellationToken);
            return res;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsSecretManagerException(e);
        }
    }
    
    public async Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        IReadOnlyDictionary<string, string> parameters, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(parameters);
        await logger.LogAsync(
            $"Getting static secrets with parameters: {cacheKey}", 
            SeverityEnu.Trace);
        
        var cached = await cache.GetAsync<IPvNugsDynamicCredential>(
            cacheKey, cancellationToken);
        if (cached != null) return cached;
        
        var res = await provider.GetDynamicSecretAsync(
            parameters, cancellationToken);
        
        if (res == null) return null;

        var ttl = DateTime.UtcNow - res.ExpirationDateUtc;
        if (ttl.TotalSeconds <= 0) return res;
        
        await cache.SetAsync(cacheKey, res, ttl, cancellationToken);
        return res;
    }

    private string GetCacheKey(IReadOnlyDictionary<string, string> parameters)
    {
        var sb = new StringBuilder(_config.CacheKeyPrefix);
        foreach (var kvp in parameters)
        {            
            sb.Append($":{kvp.Key}={kvp.Value}");
        }
        return sb.ToString();
    }

}