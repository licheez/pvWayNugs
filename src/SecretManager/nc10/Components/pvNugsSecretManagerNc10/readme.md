# pvNugsSecretManagerNc10

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc10.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc10/)
[![.NET](https://img.shields.io/badge/.NET%20Core-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

`pvNugsSecretManagerNc10` is the provider-agnostic orchestration layer of the pvWay Secret Manager stack for .NET Core 10.

It implements `IPvNugsSecretManager` by:
- delegating all backend calls to a swappable `IPvNugsSecretProvider`
- adding **transparent caching** (static secrets cached by TTL, dynamic credentials cached by expiration)
- adding **structured logging** of every retrieval attempt
- **normalizing exceptions** from any provider into `PvNugsSecretManagerException`

Your application code depends only on `IPvNugsSecretManager` and stays completely decoupled from the underlying secret store.

## Installation

```bash
dotnet add package pvNugsSecretManagerNc10
```

Package Manager Console:

```powershell
Install-Package pvNugsSecretManagerNc10
```

## Required Companion Packages

This package depends on the following pvWay packages (pull whichever suits your stack):

| Role | Package |
|---|---|
| Contracts | `pvNugsSecretManagerNc10Abstractions` (auto-referenced) |
| Provider | `pvNugsSecretManagerNc10Azure` or `pvNugsSecretManagerNc10EnvVariables` or your own |
| Cache | `pvNugsCacheNc10Memory` (or any `IPvNugsCache` implementation) |
| Logger | `pvNugsLoggerNc10Seri` (or any `IConsoleLoggerService` implementation) |

## Target Framework

- .NET Core 10.0+

## Architecture Overview

```
Consumer App
    │
    ▼
IPvNugsSecretManager      ← this package
    │  caching + logging + exception wrapping
    ▼
IPvNugsSecretProvider     ← provider package (Azure / EnvVars / Vault / ...)
    │
    ▼
Secret Backend            ← Azure Key Vault / env vars / HashiCorp Vault / ...
```

## Quick Start

### 1. Configuration (`appsettings.json`)

```json
{
  "PvNugsSecretManagerConfig": {
    "CacheKeyPrefix": "MyApp",
    "CacheTimeToLive": "1.00:00:00"
  }
}
```

| Property | Default | Description |
|---|---|---|
| `CacheKeyPrefix` | `PvNugsSecretManagerNc10` | Prefix for all cache keys. Override per app to avoid collisions in shared caches. |
| `CacheTimeToLive` | `5 days` | TTL for cached **static** secrets. Dynamic credentials are always cached until their own `ExpirationDateUtc`. |

### 2. DI Registration (`Program.cs`)

```csharp
using pvNugsCacheNc10Memory;
using pvNugsLoggerNc10Seri;
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10Azure; // your chosen provider

var builder = WebApplication.CreateBuilder(args);

// Register cache and logger (pvWay or your own implementations)
builder.Services.TryAddPvNugsCacheNc10Memory(builder.Configuration);
builder.Services.TryAddPvNugsLoggerSeriService(builder.Configuration);

// Register your provider implementation
builder.Services.TryAddSingleton<IPvNugsSecretProvider, PvNugsAzureSecretProvider>();

// Register the provider-agnostic manager + bind PvNugsSecretManagerConfig
builder.Services.TryAddPvNugsSecretManager(builder.Configuration);
```

The injector method `TryAddPvNugsSecretManager` is idempotent: if `IPvNugsSecretManager` is
already registered, it does not overwrite the existing registration.

### 3. Retrieve a Static Secret

```csharp
using pvNugsSecretManagerNc10Abstractions;

public class DatabaseService(IPvNugsSecretManager secretManager)
{
    public async Task<string> GetConnectionStringAsync(CancellationToken ct = default)
    {
        // Parameters are provider-specific — check your provider package docs for required keys
        var parameters = new Dictionary<string, string>
        {
            ["name"] = "database-password"
        };

        var password = await secretManager.GetStaticSecretAsync(parameters, ct);

        if (password is null)
            throw new InvalidOperationException("Secret 'database-password' not found.");

        return $"Server=myserver;Database=mydb;Password={password};";
    }
}
```

### 4. Retrieve a Dynamic Credential

```csharp
public async Task<NpgsqlConnection> OpenSecureConnectionAsync(CancellationToken ct = default)
{
    var parameters = new Dictionary<string, string>
    {
        ["name"] = "app-database"
    };

    var credential = await secretManager.GetDynamicSecretAsync(parameters, ct);

    if (credential is null || DateTime.UtcNow >= credential.ExpirationDateUtc)
        throw new InvalidOperationException("Unable to obtain a valid dynamic credential.");

    var connectionString =
        $"Host=myhost;Database=mydb;Username={credential.Username};Password={credential.Password};";

    var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync(ct);
    return connection;
}
```

### 5. Retrieve Multiple Static Secrets at Once

```csharp
var parameters = new Dictionary<string, string>
{
    ["prefix"] = "myapp/prod"
};

IReadOnlyDictionary<string, string> secrets =
    await secretManager.GetStaticSecretsAsync(parameters, ct);

var apiKey   = secrets["ApiKey"];
var smtpPass = secrets["SmtpPassword"];
```

## Caching Behaviour

| Secret type | Cache TTL |
|---|---|
| Static (single or batch) | `PvNugsSecretManagerConfig.CacheTimeToLive` (default 5 days) |
| Dynamic credential | `credential.ExpirationDateUtc − DateTime.UtcNow` — never outlives the credential |
| Null / empty result | **Not cached** — next call always hits the provider |

Cache keys are built as:

```
{CacheKeyPrefix}:{param1Key}={param1Value}:{param2Key}={param2Value}
```

Parameter iteration order affects the key — use a consistent insertion order per logical call site.

## Injector API

`PvNugsSecretManagerDi` exposes:

```csharp
IServiceCollection TryAddPvNugsSecretManager(
    this IServiceCollection services,
    IConfiguration config)
```

What it does:
- binds `PvNugsSecretManagerConfig` from section `PvNugsSecretManagerConfig`
- registers `IPvNugsSecretManager` as singleton (internal implementation: `SecretManager`)

What you must register separately:
- one `IPvNugsSecretProvider` implementation (Azure / EnvVariables / Vault / custom)
- one `IPvNugsCache` implementation
- one `IConsoleLoggerService` implementation

## Exception Handling

Static secret failures are wrapped in `PvNugsSecretManagerException`:

```csharp
try
{
    var secret = await secretManager.GetStaticSecretAsync(parameters, ct);
}
catch (PvNugsSecretManagerException ex)
{
    // ex.InnerException = original provider-level exception
    // ex.Message        = "pvNugsSecretManager {deep inner message}"
    logger.LogError(ex, "Secret retrieval failed");
    throw;
}
catch (OperationCanceledException)
{
    // Cancellation is never wrapped
    throw;
}
```

> **Note:** `GetDynamicSecretAsync` does **not** wrap exceptions — provider errors propagate
> directly to support retry and circuit-breaker patterns in dynamic credential workflows.

## Security Guidance

- Never log secret values or raw credential strings
- Validate `IPvNugsDynamicCredential.ExpirationDateUtc` before every use
- Use a dedicated `CacheKeyPrefix` per application in shared-cache environments
- Pass `CancellationToken` to avoid orphaned calls during graceful shutdown
- Keep `CacheTimeToLive` short for secrets that rotate frequently

## Related Packages

| Package | Role |
|---|---|
| [`pvNugsSecretManagerNc10Abstractions`](https://www.nuget.org/packages/pvNugsSecretManagerNc10Abstractions/) | Contracts — `IPvNugsSecretManager`, `IPvNugsSecretProvider`, `IPvNugsDynamicCredential` |
| `pvNugsSecretManagerNc10Azure` | Azure Key Vault provider |
| `pvNugsSecretManagerNc10EnvVariables` | Environment variables provider |
| [`pvNugsCacheNc10Memory`](https://www.nuget.org/packages/pvNugsCacheNc10Memory/) | In-memory cache implementation |
| [`pvNugsLoggerNc10Seri`](https://www.nuget.org/packages/pvNugsLoggerNc10Seri/) | Serilog-based logger implementation |

## License

MIT — see [LICENSE](https://opensource.org/licenses/MIT).

