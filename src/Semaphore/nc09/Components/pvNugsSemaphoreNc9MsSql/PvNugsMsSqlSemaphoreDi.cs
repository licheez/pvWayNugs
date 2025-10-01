using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsSemaphoreNc9Abstractions;

namespace pvNugsSemaphoreNc9MsSql;

/// <summary>
/// Provides extension methods for registering the SQL Server-based distributed semaphore service
/// and its configuration with the .NET dependency injection container.
/// </summary>
public static class PvNugsMsSqlSemaphoreDi
{
    /// <summary>
    /// Registers the SQL Server-based distributed semaphore service and its configuration in the DI container.
    /// </summary>
    /// <param name="services">The service collection to add the semaphore service to.</param>
    /// <param name="config">The application configuration containing the semaphore settings section.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection TryAddPvNugsMsSqlSemaphore(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<PvNugsMsSqlSemaphoreConfig>(
            config.GetSection(nameof(PvNugsMsSqlSemaphoreConfig)));
        services.TryAddSingleton<IPvNugsSemaphoreService, SemaphoreService>();
        return services;
    }
}