# pvNugsCsProviderNc6MsSql

**Enterprise-grade SQL Server connection string provider with multiple authentication modes, secret management integration, and multi-database support for .NET 6+**

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsCsProviderNc6MsSql)](https://www.nuget.org/packages/pvNugsCsProviderNc6MsSql)
[![.NET 6](https://img.shields.io/badge/.NET-6.0-blue)](https://dotnet.microsoft.com/download/dotnet/6.0)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## 🚀 **What is pvNugsCsProviderNc6MsSql?**

A robust, production-ready SQL Server connection string provider that seamlessly integrates with dependency injection and supports multiple authentication modes. Whether you need simple configuration-based connections, static secret management, dynamic credential rotation, or multi-database/named connection support, this provider has you covered.

> **New in vNext:**
> - **Multi-database/named connection support** via the `Rows` property in config
> - **Full backward compatibility** with legacy single-row config
> - **IOptions pattern compliance** for named/multi-tenant scenarios

## ✨ **Key Features**

### 🔐 **Multiple Authentication Modes**
- **Config Mode** - Traditional configuration-based authentication with appsettings.json
- **StaticSecret Mode** - Integration with secret managers (Azure Key Vault, HashiCorp Vault, etc.)
- **DynamicSecret Mode** - Automatic credential rotation with temporary credentials

### 🎯 **Role-Based Database Access**
- **Owner** - Full administrative privileges
- **Application** - Standard application-level permissions
- **Reader** - Read-only access for reporting and analytics

### 🏗️ **Enterprise-Ready**
- **Dependency Injection** - First-class support for .NET DI container
- **Thread-Safe** - Concurrent access with built-in locking mechanisms
- **Async/Await** - Full async support for high-performance applications
- **Automatic Renewal** - Dynamic credentials refresh before expiration
- **Comprehensive Logging** - Built-in diagnostic and error logging
- **Multi-Database/Named Connection Support** - Configure and use multiple named connection rows in a single config

## 📦 **Installation**

```shell script
dotnet add package pvNugsCsProviderNc6MsSql
```


## 🛠️ **Quick Start**

### 1. **Basic Configuration Mode (Single Row)**

```csharp
// Program.cs or Startup.cs
services.AddSingleton<IConsoleLoggerService, ConsoleLoggerServiceImpl>();
services.TryAddPvNugsCsProviderMsSql(configuration);

// appsettings.json
{
  "PvNugsCsProviderMsSqlConfig": {
    "Mode": "Config",
    "Server": "myserver.database.windows.net",
    "Database": "MyDatabase",
    "Username": "myuser",
    "Password": "mypassword",
    "UseIntegratedSecurity": false
  }
}
```

### 2. **Multi-Database & Named Connection Support**

You can now configure multiple named connection rows for multi-tenant or multi-database scenarios:

```json
{
  "PvNugsCsProviderMsSqlConfig": {
    "Rows": [
      {
        "Name": "Default",
        "Mode": "Config",
        "Server": "myserver.database.windows.net",
        "Database": "MyDatabase",
        "Username": "myuser",
        "Password": "mypassword"
      },
      {
        "Name": "Reporting",
        "Mode": "Reader",
        "Server": "reporting.database.windows.net",
        "Database": "ReportingDb",
        "Username": "reportuser",
        "Password": "reportpass"
      }
    ]
  }
}
```

**Usage Example:**

```csharp
// Get connection string for a named row
var reportingCs = await csProvider.GetConnectionStringAsync("Reporting", SqlRoleEnu.Reader);
```

> **Note:** The provider is fully backward compatible. If you use the old flat config style, it will be mapped to a single row named "Default".

### 3. **Usage in Your Services**

```csharp
public class DataService
{
    private readonly IPvNugsMsSqlCsProvider _csProvider;
    
    public DataService(IPvNugsMsSqlCsProvider csProvider)
    {
        _csProvider = csProvider;
    }
    
    public async Task<List<User>> GetUsersAsync()
    {
        // Get read-only connection for queries
        var connectionString = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
        
        using var connection = new SqlConnection(connectionString);
        // Your data access logic here...
    }
    
    public async Task SaveUserAsync(User user)
    {
        // Get application-level connection for writes
        var connectionString = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Application);
        
        using var connection = new SqlConnection(connectionString);
        // Your save logic here...
    }
}
```


## 🔧 **Advanced Configuration**

### **Static Secret Mode with Azure Key Vault**

```csharp
// Register secret manager
services.AddSingleton<IPvNugsStaticSecretManager, AzureKeyVaultSecretManager>();
services.AddSingleton<IConsoleLoggerService, ConsoleLoggerServiceImpl>();
services.TryAddPvNugsCsProviderMsSql(configuration);
```


```json
{
  "PvNugsCsProviderMsSqlConfig": {
    "Mode": "StaticSecret",
    "Server": "myserver.database.windows.net",
    "Database": "MyDatabase", 
    "Username": "myuser",
    "SecretName": "myapp-sqlserver",
    "ApplicationName": "MyApp",
    "TimeoutInSeconds": 30
  }
}
```


### **Dynamic Secret Mode with HashiCorp Vault**

```csharp
// Register dynamic secret manager  
services.AddSingleton<IPvNugsDynamicSecretManager, HashiCorpVaultDynamicSecretManager>();
services.AddSingleton<IConsoleLoggerService, ConsoleLoggerServiceImpl>();
services.TryAddPvNugsCsProviderMsSql(configuration);
```


```json
{
  "PvNugsCsProviderMsSqlConfig": {
    "Mode": "DynamicSecret",
    "Server": "myserver.database.windows.net",
    "Database": "MyDatabase",
    "SecretName": "myapp-sqlserver",
    "ApplicationName": "MyApp"
  }
}
```


## 🎯 **SQL Roles Explained**

| Role | Use Case | Permissions |
|------|----------|-------------|
| `Reader` | Analytics, reporting, read-only queries | SELECT only |
| `Application` | Standard business operations | SELECT, INSERT, UPDATE, DELETE |
| `Owner` | Migrations, schema changes, admin tasks | Full database access |

```csharp
// Use appropriate role for the task
var readerCs = await csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
var appCs = await csProvider.GetConnectionStringAsync(SqlRoleEnu.Application);  
var ownerCs = await csProvider.GetConnectionStringAsync(SqlRoleEnu.Owner);
```


## 🔗 **Configuration Reference**

| Property | Required | Description |
|----------|----------|-------------|
| `Rows` | ❌ | List of named connection rows (multi-database support) |
| `Mode` | ✅ | Authentication mode: `Config`, `StaticSecret`, or `DynamicSecret` |
| `Server` | ✅ | SQL Server hostname or IP address |
| `Database` | ✅ | Target database name |
| `Port` | ❌ | SQL Server port (default: 1433) |
| `Username` | ⚠️* | Username for authentication |
| `Password` | ❌ | Password (Config mode only) |
| `SecretName` | ⚠️* | Base secret name for secret managers |
| `UseIntegratedSecurity` | ❌ | Use Windows Authentication (default: false) |
| `ApplicationName` | ❌ | Application name in connection string |
| `TimeoutInSeconds` | ❌ | Connection timeout (default: 15) |

**\* Required based on mode:**
- **Config Mode**: Username required when not using integrated security
- **StaticSecret Mode**: Username and SecretName required
- **DynamicSecret Mode**: SecretName required

> **Backward Compatibility:**
> - If you use the old flat config style, it is automatically mapped to a single row named "Default".
> - The provider is fully compliant with the IOptions pattern and supports named options for multi-tenant/multi-db scenarios.

## 🏢 **Enterprise Scenarios**

### **Multi-Tenant Applications**
```csharp
// Different databases per tenant
services.Configure<PvNugsCsProviderMsSqlConfig>("Tenant1", config => { /* config */ });
services.Configure<PvNugsCsProviderMsSqlConfig>("Tenant2", config => { /* config */ });
```


### **High-Availability Environments**
```json
{
  "Server": "sqlcluster.company.com,1433",
  "Database": "ProductionDB",
  "ApplicationName": "MyApp-Production",
  "TimeoutInSeconds": 60
}
```


### **Development vs Production**
```csharp
#if DEBUG
    services.TryAddPvNugsCsProviderMsSql(configuration.GetSection("Development"));
#else  
    services.TryAddPvNugsCsProviderMsSql(configuration.GetSection("Production"));
#endif
```


## 📋 **Dependencies**

- **.NET 6.0+** - .NET 6 runtime
- **Microsoft.Extensions.DependencyInjection** - DI container support
- **Microsoft.Extensions.Configuration** - Configuration binding
- **pvNugsCsProviderNc6Abstractions** - Core abstractions
- **pvNugsLoggerNc6Abstractions** - Logging interfaces

### **Optional Dependencies**
- **pvNugsSecretManagerNc6Abstractions** - For secret management modes
- Your secret manager implementation (Azure Key Vault, HashiCorp Vault, etc.)

## 🛡️ **Security Best Practices**

1. **Never store passwords in configuration files** in production
2. **Use StaticSecret or DynamicSecret modes** for production environments
3. **Implement role-based access** - use Reader for queries, Application for business logic
4. **Enable connection encryption** in your connection strings
5. **Monitor credential expiration** with the built-in logging

## 🔍 **Troubleshooting**

### **Common Issues**

**❌ "IPvNugsStaticSecretManager not registered"**
```csharp
// Fix: Register the required secret manager
services.AddSingleton<IPvNugsStaticSecretManager, YourSecretManagerImpl>();
```


**❌ "Username is required for StaticSecret mode"**
```json
// Fix: Add username to configuration
{
  "Username": "your-database-user"
}
```


**❌ Connection timeout issues**
```json
// Fix: Increase timeout for slow networks
{
  "TimeoutInSeconds": 60
}
```


## 📚 **Related Packages**

- **pvNugsLoggerNc6Abstractions** - Logging abstractions
- **pvNugsSecretManagerNc6Abstractions** - Secret management abstractions
- **pvNugsConsoleLoggerNc6** - Console logging implementation

## 🤝 **Contributing**

We welcome contributions! Please see our [contributing guidelines](CONTRIBUTING.md) for details.

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 **Support**

- 📖 **Documentation**: [Full API Documentation](https://docs.example.com)
- 🐛 **Issues**: [GitHub Issues](https://github.com/yourorg/pvNugsCsProviderNc6MsSql/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/yourorg/pvNugsCsProviderNc6MsSql/discussions)

---

**Made with ❤️ for enterprise .NET applications**