# 🗄️ pvNugs Cache Nc6 Local

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![.NET](https://img.shields.io/badge/.NET-6.0-purple.svg) ![NuGet](https://img.shields.io/badge/NuGet-v6.0.0-blue.svg)

## 📋 Overview

**pvNugsCacheNc6Local** is a high-performance, thread-safe in-memory caching solution for .NET 6 applications. Built on top of `Microsoft.Extensions.Caching.Memory`, it provides a simple yet powerful abstraction for storing and retrieving cached data with configurable time-to-live (TTL) settings and comprehensive logging support.

## ✨ Key Features

- ✅ **In-Memory Caching** - Fast, efficient local caching using IMemoryCache
- ⏱️ **Configurable TTL** - Set default or per-item time-to-live durations
- 🔒 **Thread-Safe** - Concurrent access support out of the box
- 📊 **Integrated Logging** - Built-in trace logging for cache hits and misses
- 🎯 **Type-Safe** - Generic methods for strongly-typed cache operations
- ⚡ **Async/Await** - Full async support with cancellation tokens
- 🔧 **Easy Integration** - Simple dependency injection setup
- 🌐 **Configuration-Based** - Configure via appsettings.json

## 📦 Installation

```bash
dotnet add package pvNugsCacheNc6Local
```

## 🚀 Quick Start

### 1️⃣ Configure in appsettings.json

```json
{
  "PvNugsCacheConfig": {
    "DefaultTimeToLive": "00:10:00"
  }
}
```

### 2️⃣ Register Services

```csharp
using pvNugsCacheNc6Local;

var builder = WebApplication.CreateBuilder(args);

// Register the cache service
builder.Services.TryAddPvNugsCacheNc6Local(builder.Configuration);

var app = builder.Build();
```

### 3️⃣ Use the Cache

```csharp
using pvNugsCacheNc6Abstractions;

public class MyService
{
    private readonly IPvNugsCache _cache;

    public MyService(IPvNugsCache cache)
    {
        _cache = cache;
    }

    public async Task<User> GetUserAsync(string userId)
    {
        var cacheKey = $"user:{userId}";
        
        // Try to get from cache
        var cachedUser = await _cache.GetAsync<User>(cacheKey);
        if (cachedUser != null)
            return cachedUser;
        
        // Load from database
        var user = await LoadUserFromDatabaseAsync(userId);
        
        // Store in cache with 5-minute TTL
        await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));
        
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        await SaveUserToDatabaseAsync(user);
        
        // Invalidate cache
        await _cache.RemoveAsync($"user:{user.Id}");
    }
}
```

## 🎯 API Reference

### IPvNugsCache Interface

#### SetAsync
```csharp
Task SetAsync<TValue>(
    string key, 
    TValue value,
    TimeSpan? timeToLive = null,
    CancellationToken cancellationToken = default)
```
Stores a value in the cache with an optional time-to-live.

**Parameters:**
- `key` - Unique cache key (required)
- `value` - Value to cache
- `timeToLive` - Optional TTL (uses default if not specified)
- `cancellationToken` - Cancellation token

#### GetAsync
```csharp
Task<TValue?> GetAsync<TValue>(
    string key,
    CancellationToken cancellationToken = default)
```
Retrieves a value from the cache.

**Parameters:**
- `key` - Cache key to retrieve
- `cancellationToken` - Cancellation token

**Returns:** The cached value or default(TValue) if not found

#### RemoveAsync
```csharp
Task RemoveAsync(
    string key, 
    CancellationToken cancellationToken = default)
```
Removes a cached item.

**Parameters:**
- `key` - Cache key to remove
- `cancellationToken` - Cancellation token

## ⚙️ Configuration

### PvNugsCacheConfig

| Property | Type | Description |
|----------|------|-------------|
| `DefaultTimeToLive` | `TimeSpan?` | Default expiration time for cached items. If null, items don't expire automatically. |

### Configuration Example

```json
{
  "PvNugsCacheConfig": {
    "DefaultTimeToLive": "01:00:00"  // 1 hour default TTL
  }
}
```

## 🔍 Logging

The cache service integrates with the pvNugs logging framework to provide trace-level logging:

- **Cache Hit** - Logged when an item is found in cache
- **Cache Miss** - Logged when an item is not found
- **Cache Set** - Logged when an item is stored

Example log output:
```
[Trace] Cache hit for key: user:12345
[Trace] Cache miss for key: product:67890
[Trace] Cache set for key: order:54321 with TTL: 00:05:00
```

## 💡 Best Practices

### ✅ Do's

- **Use meaningful cache keys** - Include entity type and ID (e.g., `user:123`, `order:456`)
- **Set appropriate TTLs** - Consider data volatility and freshness requirements
- **Invalidate on updates** - Remove cached items when underlying data changes
- **Handle null returns** - Always check for null when retrieving cached values
- **Use generic types** - Leverage type safety with `GetAsync<T>`

### ❌ Don'ts

- **Don't cache sensitive data** - Avoid storing passwords or tokens
- **Don't use overly long TTLs** - Balance performance with data freshness
- **Don't ignore cancellation tokens** - Support cooperative cancellation
- **Don't cache large objects** - Be mindful of memory consumption

## 🔗 Dependencies

- **Microsoft.Extensions.Caching.Memory** (6.0.x) - Core caching functionality
- **Microsoft.Extensions.Options.ConfigurationExtensions** (6.0.x) - Configuration binding
- **pvNugsCacheNc6Abstractions** - Cache interface definitions
- **pvNugsLoggerNc6Abstractions** - Logging abstractions

## 📚 Related Packages

- **pvNugsCacheNc6Abstractions** - Cache interface definitions
- **pvNugsLoggerNc6Console** - Console logger implementation
- **pvNugsLoggerNc6MsSql** - SQL Server logger implementation

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## 📄 License

This project is licensed under the MIT License.

## 👨‍💻 Author

**Pierre Van Wallendael** - [pvWay Ltd](https://github.com/licheez/pvWayNugs)

## 📞 Support

For issues, questions, or suggestions:
- 🐛 [GitHub Issues](https://github.com/licheez/pvWayNugs/issues)
- 📧 Contact: [pvWay Ltd](https://github.com/licheez/pvWayNugs)

---

⭐ If you find this package useful, please consider giving it a star on GitHub!

