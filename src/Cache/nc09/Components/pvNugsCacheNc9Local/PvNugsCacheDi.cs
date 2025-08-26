using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCacheNc9Abstractions;

namespace pvNugsCacheNc9Local;

public static class PvNugsCacheDi
{
    public static IServiceCollection TryAddPvNugsCacheNc9Local(
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