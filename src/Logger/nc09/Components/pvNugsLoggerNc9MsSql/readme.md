# pvNugsLoggerNc9MsSql

A robust Microsoft SQL Server logging implementation for .NET 9+ applications, providing structured, contextual logging with automatic table management, advanced configuration, and comprehensive error handling.

## 🚀 Features

- **Secure SQL Server Integration** – Parameterized queries prevent SQL injection
- **Automatic Table Management** – Optional table creation and schema validation
- **Thread-Safe Lazy Initialization** – Efficient startup with concurrent access support
- **Contextual Logging** – Track user, company, topic, and detailed source information
- **Log Purging** – Built-in retention policy management with configurable purge operations
- **Flexible Configuration** – Customizable table structure, column names, and column lengths
- **Multi-Database Support** – Use `ConnectionStringName` to select which connection string to use for logging
- **Rich Metadata** – Machine name, method context, file path, and line number tracking
- **Advanced Indexing** – Configurable indexes for performance and query optimization
- **Multiple Interface Support** – Works with generic `ILoggerService` or specific `IMsSqlLoggerService`
- **Enterprise Ready** – Production-tested with comprehensive error handling

## 📦 Installation
```shell
dotnet add package pvNugsLoggerNc9MsSql
```

## 🔧 Quick Start

### 1. Configure in `appsettings.json`
```json
{
  "PvNugsMsSqlLogWriterConfig": {
    "ConnectionStringName": "Default", // Name of the connection string to use for logging
    "TableName": "ApplicationLogs",
    "SchemaName": "dbo",
    "CreateTableAtFirstUse": true,
    "CheckTableAtFirstUse": true,
    "UserIdColumnName": "UserId",
    "UserIdColumnLength": 128,
    "CompanyIdColumnName": "CompanyId",
    "CompanyIdColumnLength": 128,
    "SeverityCodeColumnName": "SeverityCode",
    "MessageColumnName": "Message",
    "MachineNameColumnName": "MachineName",
    "MachineNameColumnLength": 128,
    "TopicColumnName": "Topic",
    "TopicColumnLength": 128,
    "ContextColumnName": "Context",
    "ContextColumnLength": 1024,
    "CreateDateUtcColumnName": "CreateDateUtc",
    "IncludeDateIndex": true,
    "IncludePurgeIndex": true,
    "IncludeUserIndex": false,
    "IncludeTopicIndex": false,
    "DefaultRetentionPeriodForFatal": "730.00:00:00",
    "DefaultRetentionPeriodForError": "180.00:00:00",
    "DefaultRetentionPeriodForWarning": "60.00:00:00",
    "DefaultRetentionPeriodForInfo": "14.00:00:00",
    "DefaultRetentionPeriodForDebug": "1.00:00:00",
    "DefaultRetentionPeriodForTrace": "01:00:00"
  },
  "PvNugsLoggerConfig": {
    "MinLevel": "Info"
  }
}
```

### 2. Register Services in `Program.cs`
```csharp
using pvNugsLoggerNc9MsSql;
using pvNugsLoggerNc9Seri;
using pvNugsCsProviderNc9MsSql;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Register SeriLog logger service
builder.Services.TryAddPvNugsLoggerSeriService(config);

// Register MsSql connection string provider
builder.Services.TryAddPvNugsCsProviderMsSql(config);

// Register SQL Server logging services
builder.Services.TryAddPvNugsMsSqlLogger(config);

var app = builder.Build();
```

### 3. Use in Your Services
```csharp
public class OrderService
{
    private readonly IMsSqlLoggerService _logger;

    public OrderService(IMsSqlLoggerService logger)
    {
        _logger = logger;
    }

    public async Task ProcessOrderAsync(int orderId, string userId)
    {
        _logger.SetUser(userId, "company123");
        _logger.SetTopic("OrderProcessing");

        try
        {
            _logger.Log($"Processing order {orderId}", SeverityEnu.Info);
            // Your business logic here...
            await _logger.LogAsync("Order processed successfully", SeverityEnu.Info);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(ex); // Automatically captures context
            throw;
        }
    }
}
```

## 📊 Database Schema

The package automatically creates a table with this structure (fully customizable via configuration):

```sql
CREATE TABLE [dbo].[ApplicationLogs] (
    [UserId] VARCHAR(128),          -- Optional user context (configurable length)
    [CompanyId] VARCHAR(128),       -- Optional company context (configurable length)
    [SeverityCode] CHAR(1),         -- Log level (D/I/W/E/C)
    [MachineName] VARCHAR(128),     -- Server/machine name (configurable length)
    [Topic] VARCHAR(128),           -- Optional categorization (configurable length)
    [Context] VARCHAR(1024),        -- Method name, file, line number (configurable length)
    [Message] NVARCHAR(MAX),        -- Log message content
    [CreateDateUtc] DATETIME        -- UTC timestamp
)
```

## ⚙️ Advanced Configuration

### Custom Table Structure with Column Lengths
```json
{
  "PvNugsMsSqlLogWriterConfig": {
    "ConnectionStringName": "LoggingDb", // Use a named connection string for logging
    "TableName": "CustomLogs",
    "SchemaName": "audit",
    "UserIdColumnName": "UserName",
    "UserIdColumnLength": 256,
    "CompanyIdColumnName": "TenantId", 
    "CompanyIdColumnLength": 64,
    "SeverityCodeColumnName": "LogLevel",
    "MessageColumnName": "LogMessage",
    "MachineNameColumnName": "HostName",
    "MachineNameColumnLength": 200,
    "TopicColumnName": "Module",
    "TopicColumnLength": 100,
    "ContextColumnName": "SourceContext",
    "ContextColumnLength": 2048,
    "CreateTableAtFirstUse": false,
    "CheckTableAtFirstUse": true,
    "IncludeDateIndex": true,
    "IncludePurgeIndex": true,
    "IncludeUserIndex": true,
    "IncludeTopicIndex": true
  }
}
```

#### ConnectionStringName

- **ConnectionStringName**: Specifies which connection string from your configuration (e.g., `appsettings.json` or environment variables) should be used for logging. This enables scenarios where your application uses multiple databases and you want to direct logs to a specific one. If not set, defaults to `Default`.

### Column Length Guidelines

Configure column lengths based on your application needs:

- **UserIdColumnLength**: 50-256 characters (depends on user ID format)
- **CompanyIdColumnLength**: 50-256 characters (depends on tenant ID format)
- **MachineNameColumnLength**: 128+ characters (for container/cloud environments)
- **TopicColumnLength**: 50-200 characters (based on categorization needs)
- **ContextColumnLength**: 1024-2048 characters (based on file path lengths)

Values exceeding column lengths are automatically truncated with "..." suffix.

### Log Purging
```csharp
public class MaintenanceService
{
    private readonly IMsSqlLoggerService _logger;

    public async Task PurgeOldLogsAsync()
    {
        var retentionPolicies = new Dictionary<SeverityEnu, TimeSpan>
        {
            { SeverityEnu.Fatal, TimeSpan.FromDays(730) },
            { SeverityEnu.Error, TimeSpan.FromDays(180) },
            { SeverityEnu.Warning, TimeSpan.FromDays(60) },
            { SeverityEnu.Info, TimeSpan.FromDays(14) },
            { SeverityEnu.Debug, TimeSpan.FromDays(1) },
            { SeverityEnu.Trace, TimeSpan.FromHours(1) }
        };

        int deletedRows = await _logger.PurgeLogsAsync(retentionPolicies);
        Console.WriteLine($"Purged {deletedRows} old log entries");
    }
}
```

### Service Registration Order

Register the following services in your DI container for full functionality:

```csharp
services.TryAddPvNugsLoggerSeriService(config); // Console/SeriLog logging
services.TryAddPvNugsCsProviderMsSql(config);   // Connection string provider for MsSql
services.TryAddPvNugsMsSqlLogger(config);       // Main MsSql logger service
```

This ensures:
- Console and SeriLog logging is available for diagnostics and fallback.
- The MsSql connection string provider is available for secure database access.
- The MsSql logger service is fully configured and ready for use.

## 🏗️ Architecture

This package is part of the **pvNugsLogger** ecosystem:

- **pvNugsLoggerNc9Abstractions** – Core interfaces and base functionality
- **pvNugsLoggerNc9MsSql** – SQL Server implementation (this package)
- **pvNugsLoggerNc9Console** – Console logging implementation
- **pvNugsCsProviderNc9** – Connection string provider abstractions

## 🔒 Security Features

- **SQL Injection Protection** – All database operations use parameterized queries
- **Configurable Permissions** – Separate connection strings for read/write/admin operations
- **Input Validation** – Comprehensive validation with automatic string truncation
- **Error Isolation** – Logging failures don't crash your application

## 📈 Performance

- **Singleton Lifetime** – Efficient resource usage with shared instances
- **Lazy Initialization** – Tables created/validated only when needed
- **Connection Pooling** – Leverages SQL Server connection pooling
- **Async Operations** – Non-blocking logging operations
- **Optimized Column Sizing** – Configurable lengths prevent over-allocation
- **Advanced Indexing** – Date, purge, user, and topic indexes for query optimization

## 🧪 Testing Support

- Mockable interfaces for unit testing
- Supports integration testing with in-memory or test SQL Server instances
- Comprehensive error reporting for diagnostics

## 🧪 Integration Test Example

This example demonstrates how to configure and test the MsSqlLoggerService using in-memory configuration. It is ideal for integration testing or as a quick reference for setting up the logger with a specific connection string provider.

> **Note:** This sample assumes you have a SQL Server instance available (e.g., via Docker) and the appropriate connection string settings.

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9MsSql;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9MsSql;
using pvNugsLoggerNc9Seri;

Console.WriteLine("Integration testing console for pvNugsLoggerNc9MsSql");

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

var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<IConsoleLoggerService>();
var svc = sp.GetRequiredService<IMsSqlLoggerService>();

await logger.LogAsync("Logging into the Db", SeverityEnu.Trace);
await svc.LogAsync("Hello World", SeverityEnu.Trace);
await logger.LogAsync("Done", SeverityEnu.Trace);

await logger.LogAsync("Sleeping 1 second", SeverityEnu.Trace);
await Task.Delay(1000);

await logger.LogAsync("Purging", SeverityEnu.Trace);
var nbRowsPurged = await svc.PurgeLogsAsync();
await logger.LogAsync($"{nbRowsPurged} rows() purged", SeverityEnu.Trace);
```

## 📅 Migration Notes

If upgrading from a previous version, review configuration keys and schema changes. Ensure new properties (e.g., index options, column names, `ConnectionStringName`) are set as needed. See the XML documentation for full details.

---

For more details, see the XML documentation in the source code or contact the package maintainer.
