# pvNugsCacheNc10Memory

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg) ![NuGet](https://img.shields.io/badge/NuGet-v10.0.0-blue.svg)

## Overview

`pvNugsCacheNc10Memory` is a high-performance, thread-safe in-memory caching solution for .NET Core 10 applications. Built on top of `Microsoft.Extensions.Caching.Memory`, it provides a simple abstraction for storing and retrieving cached data with configurable time-to-live (TTL) settings and integrated logging.

## Key Features

- In-memory caching using `IMemoryCache`
- Configurable default and per-item TTL
- Thread-safe operations
- Trace logging for cache hit/miss/set/remove events
- Strongly-typed generic API
- Async methods with cancellation token support
- Simple dependency injection setup
- Configuration-driven defaults

## Installation

```bash
dotnet add package pvNugsCacheNc10Memory
```

## Requirements

- .NET Core 10.0 or higher
- C# 13.0 or higher

## Quick Start

### 1) Configure `appsettings.json`

```json
{
  "PvNugsCacheConfig": {
    "DefaultTimeToLive": "00:10:00"
  }
}
```

### 2) Register services

```csharp
using pvNugsCacheNc10Memory;

var builder = WebApplication.CreateBuilder(args);

// Registers IMemoryCache and IPvNugsCache
builder.Services.TryAddPvNugsCacheNc9Local(builder.Configuration);

var app = builder.Build();
```

> Note: The DI extension method currently keeps its legacy name `TryAddPvNugsCacheNc9Local` for compatibility.

### 3) Use the cache

```csharp
using pvNugsCacheNc10Abstractions;

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

        var cachedUser = await _cache.GetAsync<User>(cacheKey);
        if (cachedUser != null)
            return cachedUser;

        var user = await LoadUserFromDatabaseAsync(userId);
        await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));

        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        await SaveUserToDatabaseAsync(user);
        await _cache.RemoveAsync($"user:{user.Id}");
    }
}
```

## API Reference

### `IPvNugsCache`

#### `SetAsync`

```csharp
Task SetAsync<TValue>(
    string key,
    TValue value,
    TimeSpan? timeToLive = null,
    CancellationToken cancellationToken = default)
```

Stores a value in the cache with an optional TTL.

#### `GetAsync`

```csharp
Task<TValue?> GetAsync<TValue>(
    string key,
    CancellationToken cancellationToken = default)
```

Retrieves a value from the cache.

#### `RemoveAsync`

```csharp
Task RemoveAsync(
    string key,
    CancellationToken cancellationToken = default)
```

Removes a cached item.

## Configuration

### `PvNugsCacheConfig`

| Property | Type | Description |
|----------|------|-------------|
| `DefaultTimeToLive` | `TimeSpan?` | Default expiration time for cached items. If null, items do not expire automatically. |

## Logging

The cache integrates with the pvNugs logging abstractions and emits trace-level logs for key operations:

- Cache hit
- Cache miss
- Cache set
- Cache remove

## Dependencies

- `Microsoft.Extensions.Caching.Memory` (10.0.9)
- `Microsoft.Extensions.Options.ConfigurationExtensions` (10.0.9)
- `pvNugsCacheNc10Abstractions` (10.0.0)

## Related Packages

- `pvNugsCacheNc10Abstractions` - cache interface definitions
- `pvNugsLoggerNc10Abstractions` - logging abstractions
- `pvNugsLoggerNc10Seri` - Serilog-based logger implementation

## License

This project is licensed under the MIT License.
