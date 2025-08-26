using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCsProviderNc9Abstractions;

namespace pvNugsCsProviderNc9ByFunc;

/// <summary>
/// Provides extension methods for registering connection string provider services
/// with the dependency injection container.
/// </summary>
public static class PvNugsCsProviderDi
{
    /// <summary>
    /// Adds the connection string provider services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="getCsAsync">A function that retrieves connection strings based on SQL roles.
    /// This function will be registered as a singleton.</param>
    /// <returns>The same service collection instance for method chaining.</returns>
    /// <remarks>
    /// This method registers both the connection string retrieval function and the
    /// <see cref="IPvNugsCsProvider"/> implementation as singletons. If these services
    /// are already registered, the existing registrations will be preserved.
    /// </remarks>
    public static IServiceCollection AddPvNugsCsProvider(
        this IServiceCollection services,
        Func<SqlRoleEnu?, CancellationToken?, Task<string>> getCsAsync)
    {
        services.TryAddSingleton<
            Func<SqlRoleEnu?, CancellationToken?, Task<string>>>(
            _ => getCsAsync);
        services.TryAddSingleton<IPvNugsCsProvider, CsProvider>();
        return services;
    }
}