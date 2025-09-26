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
    /// Adds the connection string provider services to the service collection for single-database scenarios.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="getCsAsync">A function that retrieves connection strings based on SQL roles.
    /// This function will be registered as a singleton.</param>
    /// <returns>The same service collection instance for method chaining.</returns>
    /// <remarks>
    /// Registers the <see cref="IPvNugsCsProvider"/> implementation as a singleton using a function that does not support multiple database names.
    /// </remarks>
    public static IServiceCollection AddPvNugsCsProvider(
        this IServiceCollection services,
        Func<SqlRoleEnu, CancellationToken, Task<string>> getCsAsync)
    {
        services.TryAddSingleton(getCsAsync);
        services.TryAddSingleton<IPvNugsCsProvider>(sp =>
        {
            var logger = sp.GetRequiredService<pvNugsLoggerNc9Abstractions.IConsoleLoggerService>();
            return new CsProvider(logger, getCsAsync);
        });
        return services;
    }

    /// <summary>
    /// Adds the connection string provider services to the service collection for multi-database scenarios.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="getCsAsync">A function that retrieves connection strings based on database name and SQL roles.
    /// This function will be registered as a singleton.</param>
    /// <returns>The same service collection instance for method chaining.</returns>
    /// <remarks>
    /// Registers the <see cref="IPvNugsCsProvider"/> implementation as a singleton using a function that supports multiple database names.
    /// </remarks>
    public static IServiceCollection AddPvNugsCsProvider(
        this IServiceCollection services,
        Func<string, SqlRoleEnu, CancellationToken, Task<string>> getCsAsync)
    {
        services.TryAddSingleton(getCsAsync);
        services.TryAddSingleton<IPvNugsCsProvider>(sp =>
        {
            var logger = sp.GetRequiredService<pvNugsLoggerNc9Abstractions.IConsoleLoggerService>();
            return new CsProvider(logger, getCsAsync);
        });
        return services;
    }
}