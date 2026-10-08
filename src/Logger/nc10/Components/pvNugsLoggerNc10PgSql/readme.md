# pvNugsLoggerNc10PgSql

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsLoggerNc10PgSql.svg?style=flat-square)](https://www.nuget.org/packages/pvNugsLoggerNc10PgSql/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsLoggerNc10PgSql.svg?style=flat-square)](https://www.nuget.org/packages/pvNugsLoggerNc10PgSql/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

PostgreSQL-backed logger for the pvNugs ecosystem on .NET 10, with structured context capture, automatic schema/table management, and retention-based purge support.

---

## Features

- **PostgreSQL-native persistence** using `Npgsql`
- **Safe SQL execution** with parameterized commands
- **Automatic bootstrap** of schema/table/indexes (`CreateTableAtFirstUse`)
- **Optional schema validation** at startup (`CheckTableAtFirstUse`)
- **Thread-safe lazy initialization** for concurrent workloads
- **Context-rich logs** (user, company, topic, member/file/line, machine)
- **Retention-based purge** by severity with configurable defaults
- **Flexible table mapping** with customizable table/column names
- **Microsoft logging integration** via `ILoggerFactory`
- **Abstraction-first design** (`ILoggerService`, `ISqlLoggerService`, `IPgSqlLoggerService`)

---

## Installation

```bash
dotnet add package pvNugsLoggerNc10PgSql
```

---

## Security-first setup

This package relies on `pvNugsCsProviderNc10PgSql` for database connection strings.  
For production usage, prefer **StaticSecret** or **DynamicSecret** mode in the connection-string provider.

> `Config` mode stores credentials directly in configuration and is not recommended for secure environments.

---

## Quick start

### 1. Configure `appsettings.json`

```json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Rows": [
      {
        "Name": "LoggingDb",
        "Mode": "StaticSecret",
        "Server": "localhost",
        "Database": "MyApp",
        "Schema": "audit",
        "Port": 5432,
        "Username": "myapp_logger",
        "ReaderSecretParams": {
          "name": "myapp-pg-reader-password"
        },
        "ApplicationSecretParams": {
          "name": "myapp-pg-application-password"
        },
        "OwnerSecretParams": {
          "name": "myapp-pg-owner-password"
        }
      }
    ]
  },
  "PvNugsPgSqlLogWriterConfig": {
    "ConnectionStringName": "LoggingDb",
    "TableName": "logs",
    "UserIdColumnName": "UserId",
    "CompanyIdColumnName": "CompanyId",
    "SeverityCodeColumnName": "SeverityCode",
    "MachineNameColumnName": "MachineName",
    "TopicColumnName": "Topic",
    "ContextColumnName": "Context",
    "MessageColumnName": "Message",
    "CreateDateUtcColumnName": "CreateDateUtc",
    "CreateTableAtFirstUse": true,
    "CheckTableAtFirstUse": true,
    "IncludeDateIndex": true,
    "IncludePurgeIndex": true,
    "IncludeUserIndex": true,
    "IncludeTopicIndex": true,
    "DefaultRetentionPeriodForFatal": "365.00:00:00",
    "DefaultRetentionPeriodForError": "90.00:00:00",
    "DefaultRetentionPeriodForWarning": "30.00:00:00",
    "DefaultRetentionPeriodForInfo": "7.00:00:00",
    "DefaultRetentionPeriodForDebug": "1.00:00:00",
    "DefaultRetentionPeriodForTrace": "01:00:00"
  },
  "PvNugsLoggerConfig": {
    "MinLogLevel": "Info"
  }
}
```

### 2. Register services in `Program.cs`

```csharp
using pvNugsCsProviderNc10PgSql;
using pvNugsLoggerNc10PgSql;
using pvNugsLoggerNc10Seri;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Optional diagnostic console logger
builder.Services.TryAddPvNugsLoggerSeriService(config);

// PostgreSQL connection string provider (secrets-aware)
builder.Services.TryAddPvNugsCsProviderPgSql(config);

// PostgreSQL logger
builder.Services.TryAddPvNugsPgSqlLoggerNc10PgSql(config);
```

### 3. Log from application services

```csharp
public sealed class OrderService
{
    private readonly IPgSqlLoggerService _logger;

    public OrderService(IPgSqlLoggerService logger)
    {
        _logger = logger;
    }

    public async Task ProcessOrderAsync(int orderId, string userId, CancellationToken ct)
    {
        _logger.SetUser(userId, "company-001");
        _logger.SetTopic("OrderProcessing");

        await _logger.LogAsync($"Processing order {orderId}", SeverityEnu.Info, ct);

        // ... business logic

        await _logger.LogAsync($"Order {orderId} processed", SeverityEnu.Info, ct);
    }
}
```

---

## Purging logs with retention policies

`IPgSqlLoggerService` exposes purge support through `PurgeLogsAsync`.

```csharp
var retention = new Dictionary<SeverityEnu, TimeSpan>
{
    { SeverityEnu.Fatal, TimeSpan.FromDays(365) },
    { SeverityEnu.Error, TimeSpan.FromDays(90) },
    { SeverityEnu.Warning, TimeSpan.FromDays(30) },
    { SeverityEnu.Info, TimeSpan.FromDays(7) },
    { SeverityEnu.Debug, TimeSpan.FromDays(1) },
    { SeverityEnu.Trace, TimeSpan.FromHours(1) }
};

int deletedRows = await logger.PurgeLogsAsync(retention);
```

If no retention dictionary is provided, the writer uses defaults from `PvNugsPgSqlLogWriterConfig`.

---

## Generated table shape

When auto-creation is enabled, the writer creates a table similar to:

```sql
CREATE TABLE IF NOT EXISTS "audit"."logs" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "UserId" character varying(50) NULL,
    "CompanyId" character varying(128) NULL,
    "SeverityCode" character(1) NOT NULL,
    "MachineName" character varying(128) NOT NULL,
    "Topic" character varying(128) NULL,
    "Context" character varying(4096) NOT NULL,
    "Message" character varying(4096) NOT NULL,
    "CreateDateUtc" timestamp without time zone NOT NULL
);
```

Optional indexes are created based on:

- `IncludeDateIndex`
- `IncludePurgeIndex`
- `IncludeUserIndex`
- `IncludeTopicIndex`

---

## Configuration reference

### `PvNugsPgSqlLogWriterConfig`

- `ConnectionStringName` (default: `Default`)
- `TableName` (default: `Log`)
- `UserIdColumnName`
- `CompanyIdColumnName`
- `SeverityCodeColumnName`
- `MachineNameColumnName`
- `TopicColumnName`
- `ContextColumnName`
- `MessageColumnName`
- `CreateDateUtcColumnName`
- `CreateTableAtFirstUse`
- `CheckTableAtFirstUse`
- `IncludeDateIndex`
- `IncludePurgeIndex`
- `IncludeUserIndex`
- `IncludeTopicIndex`
- `DefaultRetentionPeriodForFatal`
- `DefaultRetentionPeriodForError`
- `DefaultRetentionPeriodForWarning`
- `DefaultRetentionPeriodForInfo`
- `DefaultRetentionPeriodForDebug`
- `DefaultRetentionPeriodForTrace`

### `PvNugsLoggerConfig`

- `MinLogLevel` (examples: `trace`, `debug`, `info`, `warning`, `error`, `fatal`)

---

## Service registration map

`TryAddPvNugsPgSqlLoggerNc10PgSql` registers singleton services:

- `IPgSqlLogWriter` -> `PgSqlLogWriter`
- `ISqlLogWriter` -> `IPgSqlLogWriter`
- `ILogWriter` -> `IPgSqlLogWriter`
- `ILoggerFactory` -> `PgSqlLoggerFactory`
- `IPgSqlLoggerService` -> `PgSqlLoggerService`
- `ISqlLoggerService` -> `IPgSqlLoggerService`
- `ILoggerService` -> `IPgSqlLoggerService`

---

## Architecture

This package is part of the pvNugs logging stack:

- `pvNugsLoggerNc10Abstractions` – common logger contracts and base implementation
- `pvNugsLoggerNc10PgSql` – PostgreSQL persistence implementation (this package)
- `pvNugsLoggerNc10Seri` – console/Serilog diagnostics and integration support
- `pvNugsCsProviderNc10PgSql` – PostgreSQL connection string provider (recommended with secrets)

---

## pvNugs ecosystem references (CsProvider + SecretManager)

For secure credential handling in the pvNugs ecosystem, the recommended flow is:

1. `pvNugsLoggerNc10PgSql` (this package) writes logs to PostgreSQL.
2. `pvNugsCsProviderNc10PgSql` resolves role-based connection strings (`Reader`, `Application`, `Owner`).
3. `pvNugsSecretManagerNc10` and a provider package supply secrets for `StaticSecret`/`DynamicSecret` modes.

Relevant packages:

- `pvNugsCsProviderNc10PgSql`
- `pvNugsCsProviderNc10Abstractions`
- `pvNugsSecretManagerNc10`
- `pvNugsSecretManagerNc10Abstractions`
- `pvNugsSecretManagerNc10ProviderHVault`
- `pvNugsSecretManagerNc10ProviderAzure`
- `pvNugsSecretManagerNc10ProviderEnvironment`

Typical DI composition:

```csharp
using pvNugsSecretManagerNc10ProviderHVault; // Example provider
using pvNugsCsProviderNc10PgSql;
using pvNugsLoggerNc10PgSql;

// Secret manager
builder.Services.TryAddPvNugsSecretManager(builder.Configuration);

// CsProvider uses secret manager in StaticSecret/DynamicSecret modes
builder.Services.TryAddPvNugsCsProviderPgSql(builder.Configuration);

// Logger uses CsProvider
builder.Services.TryAddPvNugsPgSqlLoggerNc10PgSql(builder.Configuration);
```

This keeps credentials out of logger configuration while preserving centralized, provider-agnostic secret management.

---

## Security notes

- Uses parameterized SQL for inserts and purge operations.
- Validates SQL identifiers (schema/table/column/index names) before use.
- Supports role-based database access through the cs-provider (`Reader`, `Application`, `Owner`).
- Prefer secret manager-backed credential modes in production.

---

## License

MIT