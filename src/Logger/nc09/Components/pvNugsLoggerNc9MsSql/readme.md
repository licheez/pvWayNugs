# pvNugsLoggerNc9MsSql

A robust Microsoft SQL Server logging implementation for .NET 9+ applications, providing structured, contextual logging with automatic table management and comprehensive error handling.

## 🚀 Features

- **Secure SQL Server Integration** - Parameterized queries prevent SQL injection attacks
- **Automatic Table Management** - Optional table creation and schema validation
- **Thread-Safe Lazy Initialization** - Efficient startup with concurrent access support
- **Contextual Logging** - Track user, company, topic, and detailed source information
- **Log Purging** - Built-in retention policy management with configurable purge operations
- **Flexible Configuration** - Customizable table structure and column names
- **Rich Metadata** - Machine name, method context, file path, and line number tracking
- **Multiple Interface Support** - Works with generic `ILoggerService` or specific `IMsSqlLoggerService`
- **Enterprise Ready** - Production-tested with comprehensive error handling

## 📦 Installation
```
shell
dotnet add package pvNugsLoggerNc9MsSql
```
## 🔧 Quick Start

### 1. Configure in `appsettings.json`
```
json
{
"PvNugsMsSqlLogWriterConfig": {
"TableName": "ApplicationLogs",
"SchemaName": "dbo",
"CreateTableAtFirstUse": true,
"CheckTableAtFirstUse": true
},
"PvNugsLoggerConfig": {
"MinLevel": "Info"
}
}
```
### 2. Register Services in `Program.cs`
```
csharp
using pvNugsLoggerNc9MsSql;

var builder = WebApplication.CreateBuilder(args);

// Register your connection string provider (implement IPvNugsMsSqlCsProvider)
builder.Services.AddSingleton<IPvNugsMsSqlCsProvider, YourConnectionProvider>();

// Register SQL Server logging services
builder.Services.TryAddPvNugsMsSqlLogger(builder.Configuration);

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
        // Set contextual information
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
```


## 📊 Database Schema

The package automatically creates a table with this structure (customizable via configuration):

```sql
CREATE TABLE [dbo].[ApplicationLogs] (
    [UserId] VARCHAR(50),           -- Optional user context
    [CompanyId] VARCHAR(50),        -- Optional company context  
    [SeverityCode] CHAR(1),         -- Log level (D/I/W/E/C)
    [MachineName] VARCHAR(100),     -- Server/machine name
    [Topic] VARCHAR(100),           -- Optional categorization
    [Context] VARCHAR(500),         -- Method name, file, line number
    [Message] NVARCHAR(MAX),        -- Log message content
    [CreateDateUtc] DATETIME        -- UTC timestamp
)
```


## ⚙️ Advanced Configuration

### Custom Table Structure

```json
{
  "PvNugsMsSqlLogWriterConfig": {
    "TableName": "CustomLogs",
    "SchemaName": "audit",
    "UserIdColumnName": "UserName",
    "CompanyIdColumnName": "TenantId",
    "SeverityCodeColumnName": "LogLevel",
    "MessageColumnName": "LogMessage",
    "CreateTableAtFirstUse": false,
    "CheckTableAtFirstUse": true
  }
}
```


### Log Purging

```csharp
public class MaintenanceService
{
    private readonly IMsSqlLoggerService _logger;

    public async Task PurgeOldLogsAsync()
    {
        var retentionPolicies = new Dictionary<SeverityEnu, TimeSpan>
        {
            { SeverityEnu.Critical, TimeSpan.FromDays(365) },
            { SeverityEnu.Error, TimeSpan.FromDays(90) },
            { SeverityEnu.Warning, TimeSpan.FromDays(30) },
            { SeverityEnu.Info, TimeSpan.FromDays(7) },
            { SeverityEnu.Debug, TimeSpan.FromDays(1) }
        };

        int deletedRows = await _logger.PurgeLogsAsync(retentionPolicies);
        Console.WriteLine($"Purged {deletedRows} old log entries");
    }
}
```


## 🏗️ Architecture

This package is part of the **pvNugsLogger** ecosystem:

- **pvNugsLoggerNc9Abstractions** - Core interfaces and base functionality
- **pvNugsLoggerNc9MsSql** - SQL Server implementation (this package)
- **pvNugsLoggerNc9Console** - Console logging implementation
- **pvNugsCsProviderNc9** - Connection string provider abstractions

## 🔒 Security Features

- **SQL Injection Protection** - All database operations use parameterized queries
- **Configurable Permissions** - Separate connection strings for read/write/admin operations
- **Input Validation** - Comprehensive validation with automatic string truncation
- **Error Isolation** - Logging failures don't crash your application

## 📈 Performance

- **Singleton Lifetime** - Efficient resource usage with shared instances
- **Lazy Initialization** - Tables created/validated only when needed
- **Connection Pooling** - Leverages SQL Server connection pooling
- **Async Operations** - Non-blocking logging operations

## 🧪 Testing Support

The package integrates seamlessly with testing frameworks:

```csharp
[Fact]
public async Task ShouldLogUserAction()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddSingleton<IMockCsProvider, MockCsProvider>();
    services.TryAddPvNugsMsSqlLogger(configuration);
    
    var provider = services.BuildServiceProvider();
    var logger = provider.GetRequiredService<IMsSqlLoggerService>();
    
    // Act
    await logger.LogAsync("Test message", SeverityEnu.Info);
    
    // Assert - verify in test database
}
```


## 📋 Requirements

- **.NET 9.0** or later
- **SQL Server 2016** or later (including Azure SQL Database)
- **Microsoft.Data.SqlClient** (automatically included)
- **Microsoft.Extensions.DependencyInjection** (automatically included)

## 🤝 Dependencies

- `pvNugsLoggerNc9Abstractions` - Core logging interfaces
- `pvNugsCsProviderNc9Abstractions` - Connection string provider interfaces
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Options`

## 📚 Documentation

Comprehensive XML documentation is included for IntelliSense support. Key interfaces:

- `IMsSqlLoggerService` - Main logging service interface
- `IMsSqlLogWriter` - Direct log writer interface
- `PvNugsMsSqlLogWriterConfig` - Configuration options class

## 🐛 Troubleshooting

### Common Issues

**Table Creation Fails**
- Ensure your connection string has CREATE TABLE permissions
- Verify the schema exists in your database

**Schema Validation Errors**
- Check that column names in configuration match your existing table
- Verify data types match expected schema (see documentation)

**Connection Issues**
- Implement `IPvNugsMsSqlCsProvider` to provide valid connection strings
- Ensure SQL Server is accessible and user has appropriate permissions

## 📄 License

This package is part of the pvNugs toolkit. See the license file for details.

## 🔗 Related Packages

- [pvNugsLoggerNc9Abstractions](https://www.nuget.org/packages/pvNugsLoggerNc9Abstractions) - Core interfaces
- [pvNugsCsProviderNc9Abstractions](https://www.nuget.org/packages/pvNugsCsProviderNc9Abstractions) - Connection providers

---

**Questions or Issues?** Please open an issue on the project repository or contact the maintainers.
