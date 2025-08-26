using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9Azure;

public static class PvNugsSecretManagerAzureDi
{
    public static IServiceCollection TryAddPvNugsAzureSecretManager(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<PvNugsAzureSecretManagerConfig>(
            config.GetSection(PvNugsAzureSecretManagerConfig.Section));
        
        services.TryAddSingleton<IPvNugsSecretManager, PvNugsSecretManager>();
        
        return services;
    }
}