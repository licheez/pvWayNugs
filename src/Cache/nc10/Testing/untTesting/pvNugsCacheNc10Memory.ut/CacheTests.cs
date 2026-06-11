using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCacheNc10Abstractions;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsCacheNc10Memory.ut;

/// <summary>
/// Unit tests for <see cref="IPvNugsCache"/> as provided by pvNugsCacheNc10Memory.
/// Services are resolved via the real DI registration to exercise the full production path.
/// </summary>
public class CacheTests
{
    // ── Fixture helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Builds a <see cref="ServiceProvider"/> with the cache stack registered and an
    /// optional default TTL.  Returns the resolved <see cref="IPvNugsCache"/>.
    /// </summary>
    private static IPvNugsCache BuildCache(TimeSpan? defaultTtl = null)
    {
        var configDict = new Dictionary<string, string?>();
        if (defaultTtl.HasValue)
            configDict["PvNugsCacheConfig:DefaultTimeToLive"] = defaultTtl.Value.ToString();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<ILoggerService, NullLoggerService>();
        services.TryAddPvNugsCacheMemory(config);

        return services.BuildServiceProvider().GetRequiredService<IPvNugsCache>();
    }

    // ── SetAsync / GetAsync ───────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsStoredValue()
    {
        var cache = BuildCache();
        await cache.SetAsync("key1", "hello");

        var result = await cache.GetAsync<string>("key1");

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task SetAsync_OverwritesExistingValue()
    {
        var cache = BuildCache();
        await cache.SetAsync("key1", "first");
        await cache.SetAsync("key1", "second");

        var result = await cache.GetAsync<string>("key1");

        Assert.Equal("second", result);
    }

    [Fact]
    public async Task SetAsync_WithComplexType_ReturnsTypedValue()
    {
        var cache = BuildCache();
        var user = new TestUser(42, "Alice");
        await cache.SetAsync("user:42", user);

        var result = await cache.GetAsync<TestUser>("user:42");

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public async Task SetAsync_WithNullKey_ThrowsArgumentException()
    {
        var cache = BuildCache();
        await Assert.ThrowsAsync<ArgumentException>(
            () => cache.SetAsync<string>(null!, "value"));
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_ThrowsArgumentException()
    {
        var cache = BuildCache();
        await Assert.ThrowsAsync<ArgumentException>(
            () => cache.SetAsync<string>(string.Empty, "value"));
    }

    // ── GetAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        var cache = BuildCache();

        var result = await cache.GetAsync<string>("no-such-key");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ReturnsDefault()
    {
        var cache = BuildCache();

        var result = await cache.GetAsync<string>(null!);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ReturnsDefault()
    {
        var cache = BuildCache();

        var result = await cache.GetAsync<string>(string.Empty);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WrongType_ReturnsDefault()
    {
        var cache = BuildCache();
        await cache.SetAsync("key1", 123); // store int

        var result = await cache.GetAsync<string>("key1"); // request string

        Assert.Null(result);
    }

    // ── RemoveAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveAsync_ExistingKey_ItemIsGone()
    {
        var cache = BuildCache();
        await cache.SetAsync("key1", "value");

        await cache.RemoveAsync("key1");

        var result = await cache.GetAsync<string>("key1");
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_NonExistentKey_DoesNotThrow()
    {
        var cache = BuildCache();
        var ex = await Record.ExceptionAsync(
            () => cache.RemoveAsync("ghost-key"));
        Assert.Null(ex);
    }

    [Fact]
    public async Task RemoveAsync_WithNullKey_DoesNotThrow()
    {
        var cache = BuildCache();
        var ex = await Record.ExceptionAsync(
            () => cache.RemoveAsync(null!));
        Assert.Null(ex);
    }

    [Fact]
    public async Task RemoveAsync_WithEmptyKey_DoesNotThrow()
    {
        var cache = BuildCache();
        var ex = await Record.ExceptionAsync(
            () => cache.RemoveAsync(string.Empty));
        Assert.Null(ex);
    }

    // ── TTL / expiry ──────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_WithExplicitShortTtl_ItemExpiresAfterTtl()
    {
        var cache = BuildCache();
        await cache.SetAsync("expiring", "value", TimeSpan.FromMilliseconds(100));

        // Still present immediately
        var before = await cache.GetAsync<string>("expiring");
        Assert.Equal("value", before);

        await Task.Delay(300);

        var after = await cache.GetAsync<string>("expiring");
        Assert.Null(after);
    }

    [Fact]
    public async Task SetAsync_WithDefaultTtl_ItemExpiresAfterDefaultTtl()
    {
        var cache = BuildCache(defaultTtl: TimeSpan.FromMilliseconds(100));
        await cache.SetAsync("expiring-default", "value");

        var before = await cache.GetAsync<string>("expiring-default");
        Assert.Equal("value", before);

        await Task.Delay(300);

        var after = await cache.GetAsync<string>("expiring-default");
        Assert.Null(after);
    }

    [Fact]
    public async Task SetAsync_WithNoTtlAndNoDefault_ItemPersists()
    {
        var cache = BuildCache(defaultTtl: null);
        await cache.SetAsync("persistent", "value");

        await Task.Delay(200);

        var result = await cache.GetAsync<string>("persistent");
        Assert.Equal("value", result);
    }

    [Fact]
    public async Task SetAsync_ExplicitTtlOverridesDefaultTtl()
    {
        // Default TTL is very short, but explicit TTL is much longer
        var cache = BuildCache(defaultTtl: TimeSpan.FromMilliseconds(50));
        await cache.SetAsync("key", "value", TimeSpan.FromSeconds(60));

        await Task.Delay(200);

        var result = await cache.GetAsync<string>("key");
        Assert.Equal("value", result);
    }

    // ── CancellationToken ─────────────────────────────────────────────────

    [Fact]
    public async Task AllMethods_AcceptCancellationToken_WithoutThrowing()
    {
        var cache = BuildCache();
        using var cts = new CancellationTokenSource();

        await cache.SetAsync("ct-key", "value", cancellationToken: cts.Token);
        var result = await cache.GetAsync<string>("ct-key", cts.Token);
        await cache.RemoveAsync("ct-key", cts.Token);

        Assert.Equal("value", result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private record TestUser(int Id, string Name);
}
