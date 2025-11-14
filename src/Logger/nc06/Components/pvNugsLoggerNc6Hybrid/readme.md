# 🚦 HybridLoggerService for .NET

A flexible, dependency-injected hybrid logger for .NET applications. The HybridLoggerService aggregates multiple log writers (console, file, SQL, etc.) and routes log messages to all configured destinations. Designed for extensibility, testability, and seamless integration with Microsoft.Extensions.DependencyInjection.

## ✨ Features

- **Aggregate Multiple Log Writers** – Console, file, SQL, and more
- **Configurable** – Set minimum log level and writer options via config or code
- **Async Logging API** – Non-blocking, scalable logging
- **Dependency Injection Ready** – Integrates with .NET DI
- **Easily Extensible** – Add custom log writers

## 📦 Installation

```shell
dotnet add package pvNugsLoggerNc6Hybrid
```

or

```shell
Install-Package pvNugsLoggerNc6Hybrid
```

## ⚡ Quick Start

### 1️⃣ Configure Logging

Add logger settings to your configuration (e.g., `appsettings.json`):

```json
{
  "PvNugsLoggerConfig": {
    "MinLogLevel": "trace"
  }
}
```

### 2️⃣ Register Services

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc6Abstractions;
using pvNugsLoggerNc6Hybrid;

var inMemSettings = new Dictionary<string, string>
{
    { "PvNugsLoggerConfig:MinLogLevel", "trace" }
    // Add other log writer settings as needed
};
var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings)
    .Build();

var services = new ServiceCollection();
services.TryAddPvNugsHybridLogger(config);
// Register other log writers as needed (console, file, SQL, etc.)

var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<ILoggerService>();
```

### 3️⃣ Log Messages

```csharp
await logger.LogAsync("Hello World from HybridLogger!", SeverityEnu.Trace);
```

## 🧪 Integration Test Example

A real integration test console using HybridLoggerService with multiple log writers and configuration:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc6MsSql;
using pvNugsLoggerNc6Abstractions;
using pvNugsLoggerNc6Hybrid;
using pvNugsLoggerNc6MsSql;
using pvNugsLoggerNc6Seri;

Console.WriteLine("Integration console for pvNugsLoggerNc6Hybrid");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    // CS PROVIDER in Config mode
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Name", "LoggingDb" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Mode", "Config" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Server", "Localhost" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Schema", "dbo" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Database", "IntTestingDb" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Port", "1433" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:TimeoutInSeconds", "300" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:UseIntegratedSecurity", "true" },
    // MS SQL LOG WRITER CONFIG
    { "PvNugsMsSqlLogWriterConfig:ConnectionStringName", "LoggingDb" },
    { "PvNugsMsSqlLogWriterConfig:DefaultRetentionPeriodForTrace", "00:00:01" },
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config);
services.TryAddPvNugsCsProviderMsSql(config);
services.TryAddPvNugsMsSqlLogger(config);
services.TryAddPvNugsHybridLogger(config);

var sp = services.BuildServiceProvider();
var cLogger = sp.GetRequiredService<IConsoleLoggerService>();
var hLogger = sp.GetRequiredService<ILoggerService>();
var sLogger = sp.GetRequiredService<IMsSqlLoggerService>();

await cLogger.LogAsync("Logging to both the console and the db ", SeverityEnu.Trace);
await hLogger.LogAsync("Hello World", SeverityEnu.Trace);
await cLogger.LogAsync("Done", SeverityEnu.Trace);
```

## ⚙️ Configuration

- Configure minimum log level and log writer options via configuration.
- Register any combination of log writers (console, file, SQL, etc.) before calling `TryAddPvNugsHybridLogger`.
- The hybrid logger will automatically aggregate all registered log writers.

## 🏗️ Architecture

- **pvNugsLoggerNc6Abstractions** – Core interfaces and base functionality
- **pvNugsLoggerNc6Hybrid** – Hybrid logger (this package)
- **pvNugsLoggerNc6Console**, **pvNugsLoggerNc6MsSql**, etc. – Pluggable log writers

## 🧪 Testing Support

- Mockable interfaces for unit testing
- Supports integration testing with in-memory or test log writers

## 📄 License

This project is licensed under the MIT License. See the LICENSE file for details.

---

For more information, see the [source repository](https://github.com/your-org/pvWayNugs).
