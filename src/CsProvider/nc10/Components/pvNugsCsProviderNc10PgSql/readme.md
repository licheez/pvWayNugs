# pvNugsCsProviderNc10PgSql

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsCsProviderNc10PgSql.svg?style=flat-square)](https://www.nuget.org/packages/pvNugsCsProviderNc10PgSql/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsCsProviderNc10PgSql.svg?style=flat-square)](https://www.nuget.org/packages/pvNugsCsProviderNc10PgSql/)
[![.NET](https://img.shields.io/badge/.NET%20Core-10.0-blue.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

**PostgreSQL Connection String Provider with Role-Based Access Control and Multi-Mode Credential Management**

A comprehensive .NET 10 library that provides secure, flexible PostgreSQL connection string management with support for three credential management modes: configuration-based, static secrets, and dynamic rotating credentials. Perfect for applications requiring role-based database access with varying security requirements.

---

## 🚀 Features

- **Three Operational Modes**
  - 🔧 **Config Mode**: Configuration-based credentials for development
  - 🔐 **StaticSecret Mode**: Secret manager integration for static passwords
  - 🔄 **DynamicSecret Mode**: Rotating credentials with automatic renewal
  
- **Role-Based Access Control**
  - Owner, Application, and Reader roles for least-privilege access
  - Per-role credential management and caching
  - Thread-safe concurrent access across roles

- **Secret Manager Integration**
  - Provider-agnostic secret manager support via `IPvNugsSecretManager`
  - Compatible with Azure Key Vault, HashiCorp Vault, AWS Secrets Manager, and custom providers
  - Flexible parameter dictionaries for provider-specific configuration

- **Dynamic Credential Management**
  - Automatic credential expiration detection
  - Configurable warning and error tolerance thresholds
  - Transparent refresh before expiration
  - Zero-downtime credential rotation

- **Performance & Reliability**
  - Per-role connection string caching
  - Double-checked locking for thread safety
  - Minimal contention with role-specific synchronization
  - Automatic cache invalidation for expired credentials

- **Configuration Flexibility**
  - Multi-database support via configuration rows
  - Support for custom ports, timezones, and timeouts
  - Optional automatic schema path configuration
  - Comprehensive validation with detailed error messages

---

## 📦 Installation

### Package Manager Console

```powershell
Install-Package pvNugsCsProviderNc10PgSql
```

### .NET CLI

```bash
dotnet add package pvNugsCsProviderNc10PgSql
```

### Package Reference

```xml
<PackageReference Include="pvNugsCsProviderNc10PgSql" Version="10.0.*" />
```

---

## 🎯 Quick Start

### 1. Install Required Packages

```bash
dotnet add package pvNugsCsProviderNc10PgSql
dotnet add package pvNugsLoggerNc10Abstractions
# Optional: Install a secret manager provider for StaticSecret or DynamicSecret modes
# dotnet add package pvNugsSecretManagerNc10ProviderHVault
```

### 2. Configure in appsettings.json

```json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "Config",
    "Server": "localhost",
    "Database": "myapp",
    "Schema": "public",
    "Port": 5432,
    "Username": "myapp_user",
    "Password": "dev_password",
    "TimeoutInSeconds": 30
  }
}
```

### 3. Register in Startup/Program.cs

```csharp
using pvNugsCsProviderNc10PgSql;

// Register required logger
services.AddSingleton<IConsoleLoggerService, YourLoggerImplementation>();

// Register the PostgreSQL connection string provider
services.TryAddPvNugsCsProviderPgSql(configuration);
```

### 4. Use in Your Services

```csharp
public class UserRepository
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;

    public UserRepository(IPvNugsPgSqlCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        // Get connection string for Reader role (least-privilege access)
        var connectionString = await _csProvider.GetConnectionStringAsync(
            CsProviderSqlRoleEnu.Reader);
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Execute your queries...
    }
}
```

---

## ⚙️ Configuration Modes

### Config Mode (Development)

Credentials stored directly in configuration. **Not recommended for production.**

```json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "Config",
    "Server": "localhost",
    "Database": "myapp_dev",
    "Schema": "public",
    "Username": "dev_user",
    "Password": "dev_password"
  }
}
```

```csharp
// Register without secret manager
services.AddSingleton<IConsoleLoggerService, ConsoleLogger>();
services.TryAddPvNugsCsProviderPgSql(configuration);
```

---

### StaticSecret Mode (Production)

Passwords retrieved from secret manager; usernames from configuration.

#### Configuration

```json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "StaticSecret",
    "Server": "prod.postgres.database.azure.com",
    "Database": "myapp_production",
    "Schema": "app_schema",
    "Port": 5432,
    "Username": "myapp_user",
    "ReaderSecretParams": {
      "name": "myapp-postgres-reader-password"
    },
    "ApplicationSecretParams": {
      "name": "myapp-postgres-app-password"
    },
    "OwnerSecretParams": {
      "name": "myapp-postgres-owner-password"
    }
  }
}
```

#### Service Registration

```csharp
// Register logger
services.AddSingleton<IConsoleLoggerService, ConsoleLogger>();

// Register secret manager (Azure Key Vault example)
services.AddSingleton<IPvNugsSecretManager, AzureKeyVaultSecretManager>();

// Register connection string provider
services.TryAddPvNugsCsProviderPgSql(configuration);
```

#### Provider-Specific Parameter Examples

**Azure Key Vault**:
```json
"ReaderSecretParams": {
  "name": "myapp-postgres-reader-password"
}
```

**HashiCorp Vault** (StaticSecret):
```json
"ReaderSecretParams": {
  "mountPoint": "secret",
  "path": "myapp/postgres",
  "key": "reader-password"
}
```

**Environment Variables**:
```json
"ReaderSecretParams": {
  "name": "MYAPP_DB_READER_PASSWORD"
}
```

---

### DynamicSecret Mode (High Security)

Both username and password dynamically generated with automatic rotation. **Requires secret manager with database secret support (e.g., HashiCorp Vault).**

#### Configuration

```json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "DynamicSecret",
    "Server": "prod.postgres.database.azure.com",
    "Database": "myapp_production",
    "Schema": "app_schema",
    "Port": 5432,
    "ReaderSecretParams": {
      "mountPoint": "database",
      "role": "myapp-reader"
    },
    "ApplicationSecretParams": {
      "mountPoint": "database",
      "role": "myapp-application"
    },
    "OwnerSecretParams": {
      "mountPoint": "database",
      "role": "myapp-owner"
    },
    "ExpirationWarningToleranceInMinutes": 30,
    "ExpirationErrorToleranceInMinutes": 5
  }
}
```

#### Service Registration

```csharp
// Register logger
services.AddSingleton<IConsoleLoggerService, ConsoleLogger>();

// Register secret manager with database secret support
// Must have SupportsDatabaseSecrets = true
services.AddSingleton<IPvNugsSecretManager, HashiCorpVaultSecretManager>();

// Register connection string provider
services.TryAddPvNugsCsProviderPgSql(configuration);
```

#### Expiration Management

- **ExpirationWarningToleranceInMinutes** (default: 30): Logs warnings when credentials approach expiration
- **ExpirationErrorToleranceInMinutes** (default: 5): Throws exceptions to force refresh before expiration

---

## 🏗️ Multi-Database Configuration

Support for multiple database configurations in a single application:

```json
{
  "PvNugsCsProviderPgSqlConfig": {
    "Rows": [
      {
        "Name": "MainDatabase",
        "Mode": "DynamicSecret",
        "Server": "main.postgres.com",
        "Database": "main_db",
        "Schema": "public",
        "ReaderSecretParams": { "role": "main-reader" }
      },
      {
        "Name": "AnalyticsDatabase",
        "Mode": "StaticSecret",
        "Server": "analytics.postgres.com",
        "Database": "analytics_db",
        "Schema": "analytics",
        "Username": "analytics_user",
        "ReaderSecretParams": { "name": "analytics-reader-pwd" }
      }
    ]
  }
}
```

```csharp
// Access specific database
var mainCs = await csProvider.GetConnectionStringAsync(
    "MainDatabase", CsProviderSqlRoleEnu.Reader);
    
var analyticsCs = await csProvider.GetConnectionStringAsync(
    "AnalyticsDatabase", CsProviderSqlRoleEnu.Reader);
```

---

## 📋 Role-Based Access

The provider supports three database roles for implementing the principle of least privilege:

| Role | Use Case | Typical Permissions |
|------|----------|-------------------|
| **Reader** | Read-only operations | SELECT |
| **Application** | Standard CRUD operations | SELECT, INSERT, UPDATE, DELETE |
| **Owner** | Schema migrations, DDL | All permissions including CREATE, DROP, ALTER |

### Example: Using Different Roles

```csharp
public class DataService
{
    private readonly IPvNugsPgSqlCsProvider _csProvider;

    public DataService(IPvNugsPgSqlCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    // Read operations use Reader role
    public async Task<List<User>> GetUsersAsync()
    {
        var cs = await _csProvider.GetConnectionStringAsync(
            CsProviderSqlRoleEnu.Reader);
        // ... execute SELECT queries
    }

    // Write operations use Application role
    public async Task CreateUserAsync(User user)
    {
        var cs = await _csProvider.GetConnectionStringAsync(
            CsProviderSqlRoleEnu.Application);
        // ... execute INSERT queries
    }

    // Schema changes use Owner role
    public async Task MigrateSchemaAsync()
    {
        var cs = await _csProvider.GetConnectionStringAsync(
            CsProviderSqlRoleEnu.Owner);
        // ... execute DDL statements
    }
}
```

---

## 🔍 Advanced Features

### Check Dynamic Credential Status

```csharp
// Check if using dynamic credentials
bool isDynamic = csProvider.UseDynamicCredentials;

// Get current cached username for a role
string username = csProvider.GetUsername(CsProviderSqlRoleEnu.Reader);

// Get schema name
string schema = csProvider.Schema;
```

### Connection String Caching

Connection strings are automatically cached per role:
- **Config/StaticSecret modes**: Cached indefinitely until application restart
- **DynamicSecret mode**: Cached until credentials approach expiration

The provider uses double-checked locking to ensure thread-safe access while minimizing contention.

---

## 🛡️ Security Best Practices

1. **Never use Config mode in production** - Credentials in plain text are a security risk
2. **Use StaticSecret mode for standard production deployments** - Keeps passwords out of configuration
3. **Use DynamicSecret mode for high-security environments** - Provides automatic credential rotation
4. **Implement role-based access** - Use Reader role by default, escalate only when needed
5. **Configure appropriate expiration tolerances** - Balance between early warnings and operational overhead
6. **Use secure transport** - Always use SSL/TLS for PostgreSQL connections
7. **Monitor credential expiration warnings** - Set up alerts for expiration warnings in DynamicSecret mode

---

## 📚 Architecture

### Component Dependencies

```
pvNugsCsProviderNc10PgSql
├── pvNugsCsProviderNc10Abstractions (interfaces)
├── pvNugsLoggerNc10Abstractions (logging)
├── pvNugsSecretManagerNc10Abstractions (secret management)
└── Microsoft.Extensions.Options.ConfigurationExtensions
```

### Dependency Injection Flow

```
Configuration (appsettings.json)
    ↓
PvNugsCsProviderPgSqlConfig (Options Pattern)
    ↓
Factory Method (Mode-Based Constructor Selection)
    ↓
CsProvider Instance
    ↓
IPvNugsCsProvider / IPvNugsPgSqlCsProvider
```

### Mode Selection Logic

```
Configuration.Mode → Config?
    ├─ Yes → Use basic constructor (logger, options)
    └─ No → StaticSecret or DynamicSecret?
        ├─ StaticSecret → Use constructor with IPvNugsSecretManager
        │                 ├─ Validate IPvNugsSecretManager is registered
        │                 └─ Use GetStaticSecretAsync for passwords
        └─ DynamicSecret → Use constructor with IPvNugsSecretManager
                           ├─ Validate IPvNugsSecretManager is registered
                           ├─ Validate SupportsDatabaseSecrets = true
                           └─ Use GetDynamicSecretAsync for username/password
```

---

## 🔧 Troubleshooting

### Common Issues

**Issue**: `InvalidOperationException: Mode StaticSecret requires a registered IPvNugsSecretManager`

**Solution**: Register a secret manager implementation before registering the provider:
```csharp
services.AddSingleton<IPvNugsSecretManager, YourSecretManagerImplementation>();
services.TryAddPvNugsCsProviderPgSql(configuration);
```

---

**Issue**: `InvalidOperationException: Mode DynamicSecret requires a secret manager that supports dynamic database secrets`

**Solution**: Ensure your secret manager has `SupportsDatabaseSecrets = true`. Only certain providers like HashiCorp Vault support dynamic database credentials.

---

**Issue**: `PvNugsCsProviderException: Username not found in configuration`

**Solution**: For StaticSecret mode, Username must be specified in configuration. DynamicSecret mode generates usernames automatically.

---

**Issue**: Connection string cached with expired credentials

**Solution**: This shouldn't happen as the provider checks expiration on every retrieval. If it does, verify your system clock is synchronized and ExpirationErrorToleranceInMinutes is configured appropriately.

---

## 🤝 Related Packages

- **[pvNugsCsProviderNc10Abstractions](https://www.nuget.org/packages/pvNugsCsProviderNc10Abstractions/)** - Interface definitions
- **[pvNugsSecretManagerNc10Abstractions](https://www.nuget.org/packages/pvNugsSecretManagerNc10Abstractions/)** - Secret manager contracts
- **[pvNugsSecretManagerNc10ProviderAzure](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderAzure/)** - Azure Key Vault provider
- **[pvNugsSecretManagerNc10ProviderHVault](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderHVault/)** - HashiCorp Vault provider
- **[pvNugsSecretManagerNc10ProviderEnvironment](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderEnvironment/)** - Environment variable provider

---

## 📄 License

MIT License - see [LICENSE](https://opensource.org/licenses/MIT) for details.

---

## 🙏 Acknowledgments

Part of the **pvWayNugs** collection of .NET utilities by pvWay Ltd.

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/licheez/pvWayNugs/issues)
- **Repository**: [https://github.com/licheez/pvWayNugs](https://github.com/licheez/pvWayNugs)
- **NuGet**: [https://www.nuget.org/packages/pvNugsCsProviderNc10PgSql](https://www.nuget.org/packages/pvNugsCsProviderNc10PgSql)

---

**Made with ❤️ for the .NET community**
