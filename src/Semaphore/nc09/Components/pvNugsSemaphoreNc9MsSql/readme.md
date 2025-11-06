# pvNugsSemaphoreNc9MsSql

[![NuGet](https://img.shields.io/nuget/v/pvNugsSemaphoreNc9MsSql.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/pvNugsSemaphoreNc9MsSql)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)](LICENSE)
[![Build Status](https://img.shields.io/github/actions/workflow/status/licheez/pvNugsSemaphoreNc9MsSql/ci.yml?branch=main&style=flat-square&logo=github)](https://github.com/licheez/pvNugsSemaphoreNc9MsSql/actions)

## ✨ Distributed Semaphore for .NET with SQL Server

`pvNugsSemaphoreNc9MsSql` provides a robust, distributed semaphore (mutex) implementation for .NET, backed by Microsoft SQL Server. It enables safe, cross-process and cross-machine locking for critical sections, scheduled jobs, or resource coordination.

---

## 🚀 Features

- **Distributed locking** across processes and servers
- **Atomic operations** using SQL Server
- **Automatic table creation** (optional)
- **Configurable** via .NET configuration
- **Async/await** support
- **Dependency Injection** ready

---

## 📦 Installation

```shell
dotnet add package pvNugsSemaphoreNc9MsSql
```

---

## ⚙️ Configuration

Add the following to your appsettings or use in-memory configuration:

```json
{
  "PvNugsMsSqlSemaphoreConfig": {
    "ConnectionStringName": "LoggingDb",
    "TableName": "MySemaphore",
    "SchemaName": "dbo",
    "CreateTableAtFirstUse": true
  }
}
```

---

## 🛠️ Usage Example

Below is a minimal integration test console showing how to configure and use the semaphore:

```csharp
uusing Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9MsSql;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsSemaphoreNc9Abstractions;
using pvNugsSemaphoreNc9MsSql;

Console.WriteLine("Integration testing console for pvNugsSemaphoreNc9MsSql");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
    // CS PROVIDER in Config mode
    // Here we mount a Docker container running postgres on port 5433
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Name", "LoggingDb" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Mode", "Config" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Server", "Localhost" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Schema", "dbo" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Database", "IntTestingDb" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Port", "1433" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:TimeoutInSeconds", "300" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:UseIntegratedSecurity", "true" },
    
    // MS SQL SEMAPHORE CONFIG
    { "PvNugsMsSqlSemaphoreConfig:ConnectionStringName", "LoggingDb" },
    { "PvNugsMsSqlSemaphoreConfig:TableName", "MySemaphore" },
    { "PvNugsMsSqlSemaphoreConfig:CreateTableAtFirstUse", "true" }
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config);
services.TryAddPvNugsCsProviderMsSql(config);
services.TryAddPvNugsMsSqlSemaphore(config);

var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<IConsoleLoggerService>();
var svc = sp.GetRequiredService<IPvNugsSemaphoreService>();

const string theMutex = "MyUniqueMutex";

var si = await svc.AcquireSemaphoreAsync(
    theMutex, Environment.MachineName, TimeSpan.FromSeconds(10));
await logger.LogAsync(si.ToString()!);
```

---

## 📚 API Overview

- `AcquireSemaphoreAsync` — Acquire or steal a named semaphore
- `ReleaseSemaphoreAsync` — Release a semaphore
- `TouchSemaphoreAsync` — Extend semaphore validity
- `GetSemaphoreAsync` — Query semaphore state
- `IsolateWorkAsync` — Run code in a protected context

See XML documentation in code for details.

---

## 📝 License

MIT © [licheez](https://github.com/licheez)

