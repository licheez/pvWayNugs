using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCacheNc6Abstractions;

namespace pvNugsCacheNc6Local;

/// <summary>
/// Dependency injection extension methods for registering the pvNugs in-memory cache service.
/// </summary>
public static class PvNugsCacheDi
{
    /// <summary>
    /// Registers the pvNugs in-memory cache service with the dependency injection container.
    /// This method uses TryAdd semantics, meaning services will only be registered if they haven't been registered already.
    /// Registers IMemoryCache and IPvNugsCache as singletons, and binds PvNugsCacheConfig from the configuration.
    /// </summary>
    /// <param name="services">The service collection to add the cache services to</param>
    /// <param name="config">The configuration instance used to bind PvNugsCacheConfig settings</param>
    /// <returns>The service collection for method chaining</returns>
    /// <example>
    /// <code>
    /// services.TryAddPvNugsCacheNc6Local(configuration);
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsCacheNc6Local(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<PvNugsCacheConfig>(
            config.GetSection(PvNugsCacheConfig.Section));

        services.TryAddSingleton<IMemoryCache, MemoryCache>();
        services.TryAddSingleton<IPvNugsCache, Cache>();
        
        return services;
    }
}