using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10ProviderHVault;

/// <summary>
/// Provides dependency injection extension methods for registering the HashiCorp Vault secret provider.
/// </summary>
public static class PvNugsHVaultSecretProviderDi
{
    /// <summary>
    /// Attempts to register the HashiCorp Vault secret provider as a singleton service.
    /// If an <see cref="IPvNugsSecretProvider"/> is already registered, this method does nothing (TryAdd semantics).
    /// </summary>
    /// <param name="services">The service collection to add the provider to.</param>
    /// <param name="config">The configuration containing the <see cref="PvNugsHVaultSecretProviderConfig"/> section.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This method configures the provider to read its settings from the "PvNugsHVaultSecretProviderConfig" section
    /// of the application configuration.
    /// </remarks>
    public static IServiceCollection
        TryAddPvNugsHVaultSecretProvider(this IServiceCollection services,
            IConfiguration config)
    {
        services.Configure<PvNugsHVaultSecretProviderConfig>(
            config.GetSection(nameof(PvNugsHVaultSecretProviderConfig)));
        
        services.TryAddSingleton<IPvNugsSecretProvider, HVaultSecretProvider>();
        
        return services;
    }
    
}