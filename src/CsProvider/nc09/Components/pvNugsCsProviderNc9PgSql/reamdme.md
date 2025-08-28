# pvNugsCsProviderPgSql

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsCsProviderPgSql.svg)](https://www.nuget.org/packages/pvNugsCsProviderPgSql/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsCsProviderPgSql.svg)](https://www.nuget.org/packages/pvNugsCsProviderPgSql/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A comprehensive PostgreSQL connection string provider for .NET 9.0 applications with support for multiple authentication modes, role-based access control, and automatic credential management.

## 🚀 Features

- **Multiple Authentication Modes**: Config-based, Static Secret Manager, and Dynamic Secret Manager
- **Role-Based Access Control**: Support for Owner, Application, and Reader roles with separate credentials
- **Automatic Credential Management**: Dynamic credential refresh with expiration handling
- **Thread-Safe Operations**: Concurrent access with internal locking and caching mechanisms
- **Flexible Configuration**: Comprehensive configuration options with validation
- **Secret Manager Integration**: Compatible with Azure Key Vault, AWS Secrets Manager, and other secret stores
- **Production Ready**: Designed for high-security environments with zero-trust architectures

## 📦 Installation
```
bash
dotnet add package pvNugsCsProviderPgSql
```
## 🛠 Dependencies

### Required Dependencies
- **IConsoleLoggerService**: Mandatory logging service for error and diagnostic logging
  ```bash
  dotnet add package pvNugsLoggerNc9Abstractions
  ```

### Optional Dependencies (Mode-Specific)
- **IPvNugsStaticSecretManager**: For StaticSecret mode
  ```bash
  dotnet add package pvNugsSecretManagerNc9Abstractions
  ```
- **IPvNugsDynamicSecretManager**: For DynamicSecret mode
  ```bash
  dotnet add package pvNugsSecretManagerNc9Abstractions
  ```

## 🔧 Quick Start

### 1. Basic Setup (Config Mode)

**appSettings.json:**
```
json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "Config",
    "Server": "localhost",
    "Database": "myapp_db",
    "Schema": "public",
    "Username": "myapp_user",
    "Password": "your_password"
  }
}
```
**Program.cs:**
```
csharp
using pvNugsCsProviderNc9PgSql;

var builder = WebApplication.CreateBuilder(args);

// Register required logger
builder.Services.AddSingleton<IConsoleLoggerService, ConsoleLoggerServiceImpl>();

// Register PostgreSQL connection string provider
builder.Services.TryAddPvNugsCsProviderPgSql(builder.Configuration);

var app = builder.Build();
```
### 2. Using the Provider
```
csharp
public class DataService
{
    private readonly IPvNugsCsProvider _csProvider;

    public DataService(IPvNugsCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        // Get connection string for Reader role (least privilege)
        var connectionString = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Your database operations...
        return users;
    }
}
```
## 🔐 Authentication Modes

### Config Mode
Credentials are stored directly in configuration files. Suitable for development environments.
```
json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "Config",
    "Server": "localhost",
    "Database": "myapp",
    "Schema": "public",
    "Username": "app_user",
    "Password": "secret123"
  }
}
```
### StaticSecret Mode
Passwords are retrieved from a secret manager while usernames come from configuration.
```
json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "StaticSecret",
    "Server": "mydb.postgres.database.azure.com",
    "Database": "production_db",
    "Schema": "app_schema",
    "Username": "app_user",
    "SecretName": "myapp-postgres"
  }
}
```

```
csharp
// Register secret manager
builder.Services.AddSingleton<IPvNugsStaticSecretManager, AzureKeyVaultSecretManager>();
builder.Services.TryAddPvNugsCsProviderPgSql(builder.Configuration);
```
### DynamicSecret Mode
Both username and password are dynamically generated with automatic expiration and renewal.
```
json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "DynamicSecret",
    "Server": "secure-db.example.com",
    "Database": "production_db",
    "Schema": "app_schema",
    "SecretName": "myapp-postgres-dynamic"
  }
}
```

```
csharp
// Register dynamic secret manager
builder.Services.AddSingleton<IPvNugsDynamicSecretManager, VaultDynamicSecretManager>();
builder.Services.TryAddPvNugsCsProviderPgSql(builder.Configuration);
```
## 🎯 Role-Based Access Control

The provider supports three SQL roles for implementing the principle of least privilege:
```
csharp
public class DatabaseService
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;

    public DatabaseService(IPvNugsPgSqlCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    // Read operations - use Reader role
    public async Task<List<Product>> GetProductsAsync()
    {
        var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
        // Use connection string...
    }

    // Application logic - use Application role
    public async Task UpdateInventoryAsync(int productId, int quantity)
    {
        var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Application);
        // Use connection string...
    }

    // Administrative tasks - use Owner role
    public async Task CreateTablesAsync()
    {
        var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Owner);
        // Use connection string...
    }
}
```
## 🔑 Secret Naming Convention

For StaticSecret and DynamicSecret modes, secrets are resolved using the pattern:
```

{SecretName}-{Role}
```
**Example:**
- SecretName: `"myapp-postgres"`
- Roles: `Reader`, `Application`, `Owner`
- Secret names: `"myapp-postgres-Reader"`, `"myapp-postgres-Application"`, `"myapp-postgres-Owner"`

## ⚙️ Configuration Options

| Property | Required | Description | Example |
|----------|----------|-------------|---------|
| `Mode` | Always | Authentication mode | `"DynamicSecret"` |
| `Server` | Always | PostgreSQL server address | `"db.example.com"` |
| `Database` | Always | Database name | `"myapp_production"` |
| `Schema` | Always | Default schema | `"app_schema"` |
| `Port` | Optional | Server port (default: 5432) | `5432` |
| `Username` | Config/Static | Database username | `"app_user"` |
| `Password` | Config only | Database password | `"secret123"` |
| `SecretName` | Secret modes | Base secret name | `"myapp-db"` |
| `Timezone` | Optional | Connection timezone | `"UTC"` |
| `TimeoutInSeconds` | Optional | Command timeout | `30` |

## 🔒 Security Best Practices

1. **Never use Config mode in production** - passwords in configuration files are a security risk
2. **Use DynamicSecret mode for high-security environments** - provides automatic credential rotation
3. **Implement role-based access** - use appropriate roles for different operations
4. **Monitor credential expiration** - the provider handles this automatically for dynamic credentials
5. **Use secure secret managers** - Azure Key Vault, AWS Secrets Manager, HashiCorp Vault

## 🚨 Error Handling

The provider throws `PvNugsCsProviderException` for various error conditions:
```
csharp
try
{
    var connectionString = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Application);
    // Use connection string...
}
catch (PvNugsCsProviderException ex)
{
    // Handle provider-specific errors
    _logger.LogError(ex, "Failed to get connection string");
}
```
Common error scenarios:
- Missing or invalid configuration
- Secret manager communication failures
- Expired or missing credentials
- Network timeouts during credential retrieval

## 🔄 Dynamic Credential Lifecycle

For DynamicSecret mode, the provider automatically:
1. Fetches fresh credentials when cache is empty
2. Monitors credential expiration times
3. Refreshes credentials before they expire
4. Handles concurrent access safely
5. Logs credential lifecycle events

## 📚 Advanced Usage

### Custom Secret Manager Integration
```
csharp
public class CustomSecretManager : IPvNugsDynamicSecretManager
{
    public async Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        string secretName, 
        CancellationToken cancellationToken = default)
    {
        // Your custom implementation
        return new DynamicCredential
        {
            Username = "generated_user",
            Password = "generated_password",
            ExpirationDateUtc = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<string?> GetStaticSecretAsync(
        string secretName, 
        CancellationToken cancellationToken = default)
    {
        // Your custom implementation
        return await GetPasswordFromCustomStore(secretName);
    }
}
```
### Monitoring and Observability

```csharp
public class MonitoredDataService
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;
    private readonly ILogger _logger;

    public MonitoredDataService(IPvNugsPgSqlCsProvider csProvider, ILogger logger)
    {
        _csProvider = csProvider;
        _logger = logger;
    }

    public async Task<string> GetDataAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
            var username = _csProvider.GetUsername(SqlRoleEnu.Reader);
            var useDynamic = _csProvider.UseDynamicCredentials;
            
            _logger.LogInformation("Retrieved connection string for user {Username}, Dynamic: {UseDynamic}, Time: {Elapsed}ms",
                username, useDynamic, stopwatch.ElapsedMilliseconds);
                
            // Use connection string...
            return "data";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connection string after {Elapsed}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```


## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions, please open an issue on the [GitHub repository](https://github.com/licheez/pvWayNugs).
