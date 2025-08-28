using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9Azure;

public static class PvNugsStaticSecretManagerAzureDi
{
    public static IServiceCollection TryAddPvNugsAzureStaticSecretManager(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<PvNugsAzureSecretManagerConfig>(
            config.GetSection(PvNugsAzureSecretManagerConfig.Section));
        
        services.TryAddSingleton<IPvNugsStaticSecretManager, PvNugsStaticSecretManager>();
        
        return services;
    }
}