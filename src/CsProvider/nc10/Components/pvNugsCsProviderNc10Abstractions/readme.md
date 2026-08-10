# pvNugs Connection String Provider Abstractions

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsCsProviderNc10Abstractions.svg)](https://www.nuget.org/packages/pvNugsCsProviderNc10Abstractions/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive .NET 10.0 abstraction library for database connection string providers with support for multiple database types, authentication modes, and role-based access control. This package provides interfaces and contracts for integrating with various database systems including PostgreSQL and Microsoft SQL Server.

## 🔐 Features

- **Multi-Database Support**: Unified abstractions for PostgreSQL and Microsoft SQL Server, now supporting multiple named database configurations.
- **Multiple Authentication Modes**: Support for config-based, static secrets, and dynamic credentials
- **Role-Based Access Control**: Built-in support for Owner, Application, and Reader roles
- **Secure Credential Management**: Integration with secret management systems
- **Thread-Safe Operations**: Designed for concurrent access in multi-threaded applications
- **Comprehensive Documentation**: Extensive XML documentation with examples and best practices

## 📦 Installation
```bash
dotnet add package pvNugsCsProviderNc10Abstractions
```
Or via Package Manager Console:
```powershell
Install-Package pvNugsCsProviderNc10Abstractions
```

## 🚀 Quick Start

### Basic Connection String Provider Usage (Multi-Database)
```csharp
public class DatabaseService
{
    private readonly IPvNugsCsProvider _csProvider;

    public DatabaseService(IPvNugsCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        // Get connection string for Reader role from the default database
        var connectionString = await _csProvider.GetConnectionStringAsync(CsProviderSqlRoleEnu.Reader);
        // Or for a specific named database:
        var auditConnectionString = await _csProvider.GetConnectionStringAsync("AuditDb", CsProviderSqlRoleEnu.Reader);
        // Use connection string with your preferred data access technology
        return users;
    }
}
```

### PostgreSQL-Specific Usage
```csharp
public class PostgreSqlService
{
    private readonly IPvNugsPgSqlCsProvider _pgProvider;

    public PostgreSqlService(IPvNugsPgSqlCsProvider pgProvider)
    {
        _pgProvider = pgProvider;
    }

    public async Task<string> GetPostgreSqlConnectionAsync()
    {
        var connectionString = await _pgProvider.GetConnectionStringAsync("MainDb", CsProviderSqlRoleEnu.Application);
        var schema = _pgProvider.GetSchema("MainDb"); // Multi-db aware
        var defaultSchema = _pgProvider.Schema; // Default database schema
        var useDynamic = _pgProvider.IsDynamicCredentials("MainDb");
        var username = _pgProvider.GetUsername("MainDb", CsProviderSqlRoleEnu.Application);
        return connectionString;
    }
}
```

### Microsoft SQL Server-Specific Usage
```csharp
public class MsSqlService
{
    private readonly IPvNugsMsSqlCsProvider _msSqlProvider;

    public MsSqlService(IPvNugsMsSqlCsProvider msSqlProvider)
    {
        _msSqlProvider = msSqlProvider;
    }

    public async Task<string> GetMsSqlConnectionAsync()
    {
        var connectionString = await _msSqlProvider.GetConnectionStringAsync("MainDb", CsProviderSqlRoleEnu.Application);
        var username = _msSqlProvider.GetUsername("MainDb", CsProviderSqlRoleEnu.Application);
        var useTrusted = _msSqlProvider.IsTrustedConnection("MainDb");
        var useDynamic = _msSqlProvider.IsDynamicCredentials("MainDb");
        return connectionString;
    }
}
```

## 🏗️ Core Interfaces

### IPvNugsCsProvider

The base interface providing fundamental connection string functionality.

**Key Features:**
- Asynchronous connection string retrieval
- Multi-database support (named configs)
- Role-based access control
- Cancellation token support
- Thread-safe operations

**Methods:**
```csharp
Task<string> GetConnectionStringAsync(string connectionStringName, CsProviderSqlRoleEnu role = CsProviderSqlRoleEnu.Reader, CancellationToken cancellationToken = default);
Task<string> GetConnectionStringAsync(CsProviderSqlRoleEnu role = CsProviderSqlRoleEnu.Reader, CancellationToken cancellationToken = default); // Default database
```

### IPvNugsPgSqlCsProvider

Extends the base provider with PostgreSQL-specific functionality.

**Additional Features:**
- Schema information access (per database)
- Dynamic credential status (per database)
- Username retrieval by role and database
- PostgreSQL-specific connection properties

**Properties and Methods:**
```csharp
string Schema { get; }
string GetSchema(string connectionStringName);
bool UseDynamicCredentials { get; }
bool IsDynamicCredentials(string connectionStringName);
string GetUsername(CsProviderSqlRoleEnu role);
string GetUsername(string connectionStringName, CsProviderSqlRoleEnu role);
```

### IPvNugsMsSqlCsProvider

Extends the base provider with Microsoft SQL Server-specific functionality.

**Additional Features:**
- Trusted connection support detection (per database)
- Username retrieval by role and database
- Dynamic credential status (per database)
- SQL Server-specific connection properties

**Properties and Methods:**
```csharp
bool UseTrustedConnection { get; }
bool IsTrustedConnection(string connectionStringName);
bool UseDynamicCredentials { get; }
bool IsDynamicCredentials(string connectionStringName);
string GetUsername(CsProviderSqlRoleEnu role);
string GetUsername(string connectionStringName, CsProviderSqlRoleEnu role);
```

## 🎯 SQL Role Enumeration

The `CsProviderSqlRoleEnu` enum defines three standard database roles for implementing the principle of least privilege:
```csharp
public enum CsProviderSqlRoleEnu
{
    Owner,       // Full administrative access
    Application, // Standard application operations
    Reader       // Read-only access
}
```

### Role Usage Guidelines

- **Reader**: Use for read-only operations (SELECT queries)
- **Application**: Use for standard CRUD operations (INSERT, UPDATE, DELETE)
- **Owner**: Use for administrative tasks (DDL operations, user management)

## 🔧 Implementation Patterns

### Dependency Injection Setup
```csharp
// Program.cs
services.AddSingleton<IPvNugsCsProvider, YourCsProviderImplementation>();
services.AddSingleton<IPvNugsPgSqlCsProvider, YourPostgreSqlProviderImplementation>();
services.AddSingleton<IPvNugsMsSqlCsProvider, YourMsSqlProviderImplementation>();
```

### Role-Based Data Access
```csharp
public class ProductService
{
    private readonly IPvNugsCsProvider _csProvider;

    public ProductService(IPvNugsCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    // Read operations - use Reader role
    public async Task<List<Product>> GetProductsAsync()
    {
        var connectionString = await _csProvider.GetConnectionStringAsync("MainDb", CsProviderSqlRoleEnu.Reader);
        // Use connection string for read operations...
    }

    // Write operations - use Application role
    public async Task<Product> CreateProductAsync(Product product)
    {
        var connectionString = await _csProvider.GetConnectionStringAsync("MainDb", CsProviderSqlRoleEnu.Application);
        // Use connection string for write operations...
    }

    // Administrative operations - use Owner role
    public async Task CreateProductTableAsync()
    {
        var connectionString = await _csProvider.GetConnectionStringAsync("MainDb", CsProviderSqlRoleEnu.Owner);
        // Use connection string for DDL operations...
    }
}
```

## 🛡️ Security Best Practices

1. **Use Role-Based Access**: Always use the minimum required role for each operation
2. **Secure Credential Storage**: Never store credentials in plain text configuration
3. **Use Dynamic Credentials**: Prefer dynamic credentials over static ones when possible
4. **Monitor Access**: Implement proper logging and monitoring for database access
5. **Regular Rotation**: Implement credential rotation policies for enhanced security

## 📚 Error Handling

Implementations should throw appropriate exceptions for provider-specific errors:
```csharp
try
{
    var connectionString = await _csProvider.GetConnectionStringAsync("MainDb", CsProviderSqlRoleEnu.Application);
    // Use connection string...
}
catch (ArgumentException ex)
{
    _logger.LogError(ex, "Failed to retrieve connection string for {Role}", CsProviderSqlRoleEnu.Application);
    // Handle configuration not found or invalid role errors
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error retrieving connection string");
    // Handle other provider-specific errors
}
```

## 🔄 Integration Examples

### With Entity Framework Core
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}

// In Program.cs or Startup.cs
var csProvider = builder.Services.BuildServiceProvider().GetRequiredService<IPvNugsCsProvider>();
var connectionString = await csProvider.GetConnectionStringAsync("MainDb", CsProviderSqlRoleEnu.Application);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // or UseNpgsql for PostgreSQL
```

### With Dapper
```csharp
public class UserRepository
{
    private readonly IPvNugsCsProvider _csProvider;

    public UserRepository(IPvNugsCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        var connectionString = await _csProvider.GetConnectionStringAsync("MainDb", CsProviderSqlRoleEnu.Reader);
        using var connection = new SqlConnection(connectionString); // or NpgsqlConnection
        return (await connection.QueryAsync<User>("SELECT * FROM Users")).ToList();
    }
}
```

## 🎯 Target Framework

- **.NET 10.0**: Built specifically for the latest .NET platform with modern language features

## 📚 Documentation

The package includes comprehensive XML documentation with:
- Detailed interface descriptions
- Method parameter explanations
- Usage examples and best practices
- Security considerations and guidelines
- Integration patterns and common use cases

## 🤝 Contributing

This package is part of the pvWayNugs ecosystem. For issues, suggestions, or contributions, please visit the [GitHub repository](https://github.com/licheez/pvWayNugs).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://opensource.org/licenses/MIT) file for details.

## 🏢 About pvWay Ltd

pvWay Ltd specializes in secure, enterprise-grade .NET solutions with a focus on security, reliability, and developer experience.

---

**Keywords**: Connection String Provider, Database Access, PostgreSQL, Microsoft SQL Server, Role-Based Access, Security, .NET 9, Abstractions, pvWayNugs
