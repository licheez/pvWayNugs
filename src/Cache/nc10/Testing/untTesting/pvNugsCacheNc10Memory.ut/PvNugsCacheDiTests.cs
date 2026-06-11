using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCacheNc10Abstractions;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsCacheNc10Memory.ut;

/// <summary>
/// Verifies the DI registration behaviour of
/// <see cref="PvNugsCacheDi.TryAddPvNugsCacheMemory"/>.
/// </summary>
public class PvNugsCacheDiTests
{
    // ── Helpers ───────────────────────────────────────────────────────────

    private static IConfiguration EmptyConfig() =>
        new ConfigurationBuilder().Build();

    private static ServiceCollection BaseServices()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ILoggerService, NullLoggerService>();
        return sc;
    }

    // ── Registration ──────────────────────────────────────────────────────

    [Fact]
    public void TryAddPvNugsCacheNc9Local_RegistersIPvNugsCache()
    {
        var services = BaseServices();
        services.TryAddPvNugsCacheMemory(EmptyConfig());

        var provider = services.BuildServiceProvider();
        var cache = provider.GetService<IPvNugsCache>();

        Assert.NotNull(cache);
    }

    [Fact]
    public void TryAddPvNugsCacheNc9Local_RegistersIMemoryCache()
    {
        var services = BaseServices();
        services.TryAddPvNugsCacheMemory(EmptyConfig());

        var provider = services.BuildServiceProvider();
        var memoryCache = provider.GetService<IMemoryCache>();

        Assert.NotNull(memoryCache);
    }

    [Fact]
    public void TryAddPvNugsCacheNc9Local_IPvNugsCache_IsSingleton()
    {
        var services = BaseServices();
        services.TryAddPvNugsCacheMemory(EmptyConfig());

        var provider = services.BuildServiceProvider();
        var instance1 = provider.GetRequiredService<IPvNugsCache>();
        var instance2 = provider.GetRequiredService<IPvNugsCache>();

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void TryAddPvNugsCacheNc9Local_IMemoryCache_IsSingleton()
    {
        var services = BaseServices();
        services.TryAddPvNugsCacheMemory(EmptyConfig());

        var provider = services.BuildServiceProvider();
        var instance1 = provider.GetRequiredService<IMemoryCache>();
        var instance2 = provider.GetRequiredService<IMemoryCache>();

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void TryAddPvNugsCacheNc9Local_CalledTwice_DoesNotOverrideRegistration()
    {
        var services = BaseServices();
        services.TryAddPvNugsCacheMemory(EmptyConfig());
        services.TryAddPvNugsCacheMemory(EmptyConfig()); // second call should be no-op

        var provider = services.BuildServiceProvider();

        // Single descriptor for IPvNugsCache
        var registrations = services.Count(sd => sd.ServiceType == typeof(IPvNugsCache));
        Assert.Equal(1, registrations);
    }

    [Fact]
    public void TryAddPvNugsCacheNc9Local_WithConfiguredTtl_BindsConfig()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PvNugsCacheConfig:DefaultTimeToLive"] = "00:05:00"
            })
            .Build();

        var services = BaseServices();
        services.TryAddPvNugsCacheMemory(config);

        var provider = services.BuildServiceProvider();

        // Cache resolves without error when config is bound
        var cache = provider.GetRequiredService<IPvNugsCache>();
        Assert.NotNull(cache);
    }
}

