# pvNugsCacheNc6Abstractions

A lightweight .NET caching abstraction library that provides a simple, async-first interface for caching operations with optional time-to-live support.

## Features

- **Async-First Design**: All operations are asynchronous with `CancellationToken` support
- **Generic Type Support**: Store and retrieve any type of data with full type safety
- **Flexible Expiration**: Optional time-to-live (TTL) configuration per cache entry
- **Simple Interface**: Clean, minimal API surface with just three core operations
- **Framework Agnostic**: Pure abstraction with no dependencies on specific cache implementations

## Installation
```
bash
dotnet add package pvNugsCacheNc9Abstractions
```
## Quick Start

### Basic Usage
```
csharp
public class UserService
{
private readonly IPvNugsCache _cache;

    public UserService(IPvNugsCache cache)
    {
        _cache = cache;
    }
    
    public async Task<User?> GetUserAsync(int userId, CancellationToken ct = default)
    {
        // Try to get from cache first
        var cachedUser = await _cache.GetAsync<User>($"user:{userId}", ct);
        if (cachedUser != null)
            return cachedUser;
            
        // Fetch from database if not in cache
        var user = await FetchUserFromDatabase(userId, ct);
        
        // Cache for 30 minutes
        await _cache.SetAsync($"user:{userId}", user, TimeSpan.FromMinutes(30), ct);
        
        return user;
    }
}
```
### Dependency Injection Setup

```csharp
// Register your cache implementation
services.AddSingleton<IPvNugsCache, YourCacheImplementation>();

// Or use with existing cache providers
services.AddMemoryCache();
services.AddSingleton<IPvNugsCache, MemoryCacheAdapter>();
```
## API Reference

### IPvNugsCache Interface

#### SetAsync<TValue>(string key, TValue value, TimeSpan? timeToLive, CancellationToken cancellationToken)

Stores a value in the cache with an optional expiration time.

- **key**: Unique identifier for the cached value
- **value**: The data to cache
- **timeToLive**: Optional expiration time (null uses default policy)
- **cancellationToken**: Cancellation token for the operation

#### GetAsync<TValue>(string key, CancellationToken cancellationToken)

Retrieves a value from the cache.

- **key**: Unique identifier for the cached value
- **cancellationToken**: Cancellation token for the operation
- **Returns**: The cached value or null if not found/expired

#### RemoveAsync(string key, CancellationToken cancellationToken)

Removes a value from the cache.

- **key**: Unique identifier for the value to remove
- **cancellationToken**: Cancellation token for the operation

## Usage Examples

### Caching with Different TTL Values

```csharp
// Short-lived cache (5 minutes)
await cache.SetAsync("temp-data", tempData, TimeSpan.FromMinutes(5));

// Long-lived cache (1 hour)
await cache.SetAsync("config", configuration, TimeSpan.FromHours(1));

// Use default TTL policy
await cache.SetAsync("default-ttl", data);
```

### Type-Safe Caching

```csharp
// Cache complex objects
var user = new User { Id = 1, Name = "John Doe" };
await cache.SetAsync("user:1", user, TimeSpan.FromMinutes(30));

// Retrieve with type safety
var cachedUser = await cache.GetAsync<User>("user:1");

// Cache collections
var users = new List<User> { user1, user2, user3 };
await cache.SetAsync("users:active", users, TimeSpan.FromMinutes(15));
```

### Cache Invalidation

```csharp
public async Task UpdateUserAsync(User user)
{
    // Update in database
    await SaveUserToDatabase(user);
    
    // Invalidate cache
    await _cache.RemoveAsync($"user:{user.Id}");
    
    // Optionally refresh cache
    await _cache.SetAsync($"user:{user.Id}", user, TimeSpan.FromMinutes(30));
}
```

## Implementation Guidelines

When implementing `IPvNugsCache`, consider:

- **Thread Safety**: Ensure your implementation is thread-safe
- **Serialization**: Handle serialization/deserialization of complex types
- **Error Handling**: Gracefully handle cache failures
- **Memory Management**: Implement proper cleanup and eviction policies
- **Monitoring**: Add logging and metrics for cache operations

## Compatible Frameworks

- .NET 6.0+
- .NET Core 3.1+
- .NET Framework 4.8+ (with nullable reference types support)

## License

MIT
