# pvNugs PostgreSQL Connection String Provider

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsCsProviderNc9PgSql.svg)](https://www.nuget.org/packages/pvNugsCsProviderNc9PgSql/)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A secure, production-ready PostgreSQL connection string provider for .NET 9.0 applications with advanced credential management, role-based access control, and automatic secret rotation capabilities.

## 🚀 Key Features

- **🔐 Multiple Authentication Modes**: Config-based, Static secrets, and Dynamic credentials with automatic rotation
- **👥 Role-Based Access Control**: Built-in support for Owner, Application, and Reader database roles
- **⚡ High Performance**: Thread-safe connection string caching with automatic refresh
- **🛡️ Advanced Security**: Configurable expiration tolerance and proactive credential validation
- **🔄 Automatic Rotation**: Seamless handling of dynamic credential expiration and renewal
- **📊 Production Ready**: Comprehensive logging, error handling, and monitoring support

## 📦 Installation
```
bash
# Package Manager Console
Install-Package pvNugsCsProviderNc9PgSql

# .NET CLI
dotnet add package pvNugsCsProviderNc9PgSql

# PackageReference
<PackageReference Include="pvNugsCsProviderNc9PgSql" Version="x.x.x" />
```
## 🏗️ Quick Start

### 1. Basic Setup (Config Mode)
```
csharp
// appsettings.json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "Config",
    "Server": "localhost",
    "Database": "myapp_db",
    "Schema": "app_schema",
    "Port": 5432,
    "Username": "myapp_user",
    "Password": "secure_password",
    "Timezone": "UTC",
    "TimeoutInSeconds": 300
  }
}

// Program.cs
services.TryAddPvNugsCsProviderPgSql(configuration);

// Usage
public class ProductService
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;
    
    public ProductService(IPvNugsPgSqlCsProvider csProvider)
    {
        _csProvider = csProvider;
    }
    
    public async Task<List<Product>> GetProductsAsync()
    {
        var connectionString = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
        // Use with Npgsql, Entity Framework, Dapper, etc.
    }
}
```
### 2. Static Secret Mode
```
csharp
// appsettings.json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "StaticSecret",
    "Server": "localhost",
    "Database": "myapp_db",
    "Schema": "app_schema",
    "Port": 5432,
    "Username": "myapp_user",
    "SecretName": "MyAppDatabase"
  }
}

// Program.cs
services.TryAddPvNugsSecretManagerEnvVariables(configuration); // or your preferred secret manager
services.TryAddPvNugsCsProviderPgSql(configuration);

// Expected secrets:
// - MyAppDatabase-Owner (password for Owner role)
// - MyAppDatabase-Application (password for Application role) 
// - MyAppDatabase-Reader (password for Reader role)
```
### 3. Dynamic Secret Mode
```
csharp
// appsettings.json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "DynamicSecret",
    "Server": "localhost", 
    "Database": "myapp_db",
    "Schema": "app_schema",
    "Port": 5432,
    "SecretName": "MyAppDynamicDb",
    "ExpirationWarningToleranceInMinutes": 30,
    "ExpirationErrorToleranceInMinutes": 5
  }
}

// Program.cs
services.TryAddPvNugsDynamicSecretManager(configuration); // your dynamic secret manager
services.TryAddPvNugsCsProviderPgSql(configuration);

// Expected dynamic secrets (with expiration):
// - MyAppDynamicDb-Owner { Username, Password, ExpirationDateUtc }
// - MyAppDynamicDb-Application { Username, Password, ExpirationDateUtc }
// - MyAppDynamicDb-Reader { Username, Password, ExpirationDateUtc }
```
## 🎯 Role-Based Access Patterns

### Principle of Least Privilege
```
csharp
public class UserService
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;

    // ✅ Read operations - use Reader role
    public async Task<List<User>> GetUsersAsync()
    {
        var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
        // SELECT queries only
    }

    // ✅ CRUD operations - use Application role  
    public async Task<User> CreateUserAsync(User user)
    {
        var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Application);
        // INSERT, UPDATE, DELETE operations
    }

    // ✅ Schema changes - use Owner role
    public async Task CreateUserIndexAsync()
    {
        var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Owner);
        // DDL operations, user management
    }
}
```
## ⚙️ Configuration Reference

### Core Settings

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Mode` | `CsProviderModeEnu` | ✅ | Authentication mode: `Config`, `StaticSecret`, or `DynamicSecret` |
| `Server` | `string` | ✅ | PostgreSQL server hostname/IP |
| `Database` | `string` | ✅ | Database name |
| `Schema` | `string` | ✅ | Default schema (added to Search Path) |
| `Port` | `int?` | ❌ | Server port (default: 5432) |
| `Timezone` | `string?` | ❌ | Connection timezone (default: UTC) |
| `TimeoutInSeconds` | `int?` | ❌ | Command timeout (default: 300) |

### Mode-Specific Settings

#### Config Mode
| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Username` | `string` | ✅ | Database username |
| `Password` | `string?` | ❌ | Database password |

#### StaticSecret Mode
| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Username` | `string` | ✅ | Database username |
| `SecretName` | `string` | ✅ | Base name for secret lookups |

#### DynamicSecret Mode
| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `SecretName` | `string` | ✅ | Base name for dynamic secret lookups |
| `ExpirationWarningToleranceInMinutes` | `int?` | ❌ | Warning threshold (default: 30) |
| `ExpirationErrorToleranceInMinutes` | `int?` | ❌ | Error threshold (default: 5) |

## 🔒 Security Features

### Dynamic Credential Validation

The provider implements a three-tier validation system for dynamic credentials:

1. **🟢 Normal Zone**: Credential is safe to use
2. **🟡 Warning Zone**: Credential approaching expiration (logs warning)
3. **🔴 Error Zone**: Credential too close to expiration (throws exception)
4. **❌ Expired**: Credential has expired (throws exception)
```
csharp
// Example: Configure custom tolerance
{
  "PvNugsCsProviderPgSqlConfig": {
    "ExpirationWarningToleranceInMinutes": 45,  // Warn 45 min before expiration
    "ExpirationErrorToleranceInMinutes": 10     // Error 10 min before expiration
  }
}
```
### Thread Safety

- ✅ Concurrent access across different roles
- ✅ Per-role semaphores prevent duplicate credential fetching
- ✅ Double-checked locking pattern for cache access
- ✅ Automatic cache invalidation for expired credentials

## 🔄 Integration Examples

### With Entity Framework Core
```
csharp
public class AppDbContext : DbContext
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;

    public AppDbContext(IPvNugsPgSqlCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _csProvider.GetConnectionStringAsync(SqlRoleEnu.Application).Result;
        optionsBuilder.UseNpgsql(connectionString);
    }
}
```
### With Dapper
```
csharp
public class ProductRepository
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;

    public async Task<List<Product>> GetProductsAsync()
    {
        var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
        using var connection = new NpgsqlConnection(cs);
        return (await connection.QueryAsync<Product>("SELECT * FROM products")).ToList();
    }
}
```
### With Raw Npgsql
```
csharp
public class OrderService
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;

    public async Task<Order> CreateOrderAsync(Order order)
    {
        var cs = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Application);
        await using var connection = new NpgsqlConnection(cs);
        await connection.OpenAsync();
        
        // Your SQL operations here
    }
}
```
## 🛠️ Testing and Integration

### Integration Testing Setup
```
csharp
// Use environment variables for testing
Environment.SetEnvironmentVariable("intTesting__MyPgSqlStaticPassword-Reader", "test_password");
Environment.SetEnvironmentVariable("intTesting__MyPgSqlDynamicCredential-Reader__Username", "test_user");
Environment.SetEnvironmentVariable("intTesting__MyPgSqlDynamicCredential-Reader__Password", "test_pass");
Environment.SetEnvironmentVariable("intTesting__MyPgSqlDynamicCredential-Reader__ExpirationDateUtc", 
    DateTime.UtcNow.AddHours(1).ToString("O"));

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(testSettings)
    .AddEnvironmentVariables()
    .Build();

services.TryAddPvNugsSecretManagerEnvVariables(config);
services.TryAddPvNugsCsProviderPgSql(config);
```
## 📊 Monitoring and Logging

The provider integrates with structured logging:
```
csharp
// Successful operations
[Trace] Retrieved connection string for role: {Role}

// Warning conditions  
[Warning] Secret 'MyApp-Reader' will expire in 25.3 minutes at 2024-01-15 14:30:00 UTC

// Error conditions
[Error] Secret 'MyApp-Application' will expire in 3.2 minutes at 2024-01-15 14:30:00 UTC
[Error] Secret 'MyApp-Owner' has expired at 2024-01-15 14:00:00 UTC
```
## 🚀 Performance Characteristics

- **Connection String Caching**: O(1) cache lookups per role
- **Thread Safety**: Minimal contention with per-role locks
- **Memory Efficient**: Bounded cache size (max 3 entries per provider instance)
- **Network Optimized**: Credentials fetched only when needed or expired

## 🔧 Troubleshooting

### Common Issues

**"StaticSecretManager has not been provisioned"**
- Ensure you've registered a static secret manager implementation
- Verify the correct constructor is being used for StaticSecret mode

**"Username not found in configuration"**
- Check that `Username` is configured for Config and StaticSecret modes
- Verify your appsettings.json structure matches the expected format

**"Secret will expire in X minutes"**
- Dynamic credentials are approaching expiration
- Consider adjusting `ExpirationErrorToleranceInMinutes` if needed
- Check your secret management system's renewal process

## 📚 Dependencies

- **Microsoft.Extensions.Options**: Configuration binding
- **pvNugsCsProviderNc9Abstractions**: Core interfaces
- **pvNugsLoggerNc9Abstractions**: Logging abstractions
- **pvNugsSecretManagerNc9Abstractions**: Secret management

## 🤝 Contributing

This package is part of the pvWayNugs ecosystem. Issues, suggestions, and contributions are welcome via [GitHub](https://github.com/licheez/pvWayNugs).

## 📄 License

MIT License - see [LICENSE](https://opensource.org/licenses/MIT) for details.

---

**Tags**: PostgreSQL, Connection String, .NET 9, Security, Role-Based Access, Dynamic Credentials, Secret Management, pvWayNugs
