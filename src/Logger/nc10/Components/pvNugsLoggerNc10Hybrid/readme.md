# pvNugsLoggerNc10Hybrid

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsLoggerNc10Hybrid.svg?style=flat-square)](https://www.nuget.org/packages/pvNugsLoggerNc10Hybrid/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsLoggerNc10Hybrid.svg?style=flat-square)](https://www.nuget.org/packages/pvNugsLoggerNc10Hybrid/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

**🧩 Hybrid logger composition for the pvNugs ecosystem (.NET 10)**  
`pvNugsLoggerNc10Hybrid` aggregates multiple registered log writers into one `ILoggerService` implementation.

---

## ✨ What this package does

`TryAddPvNugsHybridLogger(...)` scans DI for registered:

- `ILogWriter`
- `IConsoleLogWriter`
- `ISqlLogWriter`

Then it:

1. Merges these writers into a single set
2. Deduplicates them by concrete type name
3. Builds `HybridLoggerService`
4. Registers/replaces `ILoggerService` to resolve to `IHybridLoggerService`

This gives you **one logger entry point** that writes to all configured targets.

---

## 🚀 Installation

```bash
dotnet add package pvNugsLoggerNc10Hybrid
```

---

## ⚙️ Registration order (important)

Register concrete logger providers/writers first, then register hybrid:

```csharp
using pvNugsLoggerNc10Seri;
using pvNugsLoggerNc10PgSql;
using pvNugsLoggerNc10Hybrid;
using pvNugsCsProviderNc10PgSql;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1) Register concrete logger implementations first
builder.Services.TryAddPvNugsLoggerSeriService(config);         // Console/Serilog
builder.Services.TryAddPvNugsCsProviderPgSql(config);           // PgSql cs-provider
builder.Services.TryAddPvNugsPgSqlLoggerNc10PgSql(config);      // PgSql logger/writer

// 2) Register hybrid last so ILoggerService points to the aggregator
builder.Services.TryAddPvNugsHybridLogger(config);
```

---

## 🧠 How DI mapping works

After registration:

- `IHybridLoggerService` -> `HybridLoggerService` (singleton)
- `ILoggerService` -> resolves to `IHybridLoggerService` (singleton replacement/add)

So when app code injects `ILoggerService`, it gets the hybrid implementation.

---

## 🧪 Usage example

```csharp
public sealed class BillingService
{
    private readonly ILoggerService _logger;

    public BillingService(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(string userId, CancellationToken ct)
    {
        _logger.SetUser(userId, "tenant-a");
        _logger.SetTopic("Billing");

        await _logger.LogAsync("Billing started", SeverityEnu.Info, ct);
        await _logger.LogAsync("Billing finished", SeverityEnu.Info, ct);
    }
}
```

---

## 📦 pvNugs ecosystem references

Typical stack:

- `pvNugsLoggerNc10Abstractions` (contracts/base logger)
- `pvNugsLoggerNc10Seri` (console diagnostics)
- `pvNugsLoggerNc10PgSql` (database persistence)
- `pvNugsCsProviderNc10PgSql` (role-based connection strings)
- `pvNugsSecretManagerNc10` + provider packages for secure secret retrieval

The hybrid package composes writer outputs from these components into one logger service.

---

## 🔐 Security guidance

- Hybrid itself does not store secrets.
- For database logging, prefer cs-provider secret-backed modes (`StaticSecret` / `DynamicSecret`) over config-embedded credentials.

---

## 📄 License

MIT