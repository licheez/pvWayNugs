using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10ProviderAzure;

/// <summary>
/// Dependency injection helpers for the Azure secret provider.
/// </summary>
public static class PvNugsAzureSecretProviderDi
{
    /// <summary>
    /// Registers <see cref="IPvNugsSecretProvider"/> using <see cref="AzureSecretProvider"/>.
    /// </summary>
    public static IServiceCollection TryAddPvNugsAzureSecretProvider(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<PvNugsAzureSecretProviderConfig>(
            config.GetSection(nameof(PvNugsAzureSecretProviderConfig)));

        services.TryAddSingleton<IPvNugsSecretProvider, AzureSecretProvider>();

        return services;
    }
}