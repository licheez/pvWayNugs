# pvNugsCsProviderNc9Abstractions

A .NET library providing abstractions for managing database connection strings with role-based access control. This package defines the core interfaces and contracts for connection string providers without being tied to any specific database technology.

## Features

- Role-based connection string management
- Database-agnostic connection string provider abstractions
- Support for dynamic credentials
- Schema-aware configuration
- Async-first design
- Clean separation between abstractions and implementations
- .NET 9.0 target framework support

## Installation

Install the package via NuGet:

```bash
dotnet add package pvNugsCsProviderNc9Abstractions
```

## Usage

### Basic Connection String Provider

```csharp
// Inject the provider
public class MyService
{
    private readonly IPvNugsCsProvider _connectionStringProvider;

    public MyService(IPvNugsCsProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task ConnectToDatabase()
    {
        // Get connection string with default reader role
        string connectionString = await _connectionStringProvider.GetConnectionStringAsync();
        
        // Or specify a different role
        string ownerConnectionString = await _connectionStringProvider.GetConnectionStringAsync(SqlRoleEnu.Owner);
    }
}
```


### Database-Specific Provider Interface

```csharp
// Example using a database-specific provider interface
public class MyDatabaseService
{
    private readonly IDatabaseSpecificCsProvider _provider;

    public MyDatabaseService(IDatabaseSpecificCsProvider provider)
    {
        _provider = provider;
    }

    public void ConfigureConnection()
    {
        // Access database-specific properties
        string schema = _provider.Schema;
        string username = _provider.UserName;
        bool usesDynamicCreds = _provider.UseDynamicCredentials;
        SqlRoleEnu currentRole = _provider.Role;
    }
}
```

## Available Roles

The package defines three levels of database access roles:

- `SqlRoleEnu.Owner` - Highest privilege level with full database control
- `SqlRoleEnu.Application` - Standard application-level privileges with write access
- `SqlRoleEnu.Reader` - Read-only access level

## Architecture

This abstractions package provides:

- **Core interfaces** for connection string providers
- **Role enumeration** for access level management
- **Base contracts** for database-specific implementations
- **Async patterns** for modern application development

The actual database-specific implementations are provided in separate packages that depend on this abstraction layer, ensuring clean separation of concerns and testability.

## Dependency Injection

```csharp
// Register your chosen implementation
services.AddSingleton<IPvNugsCsProvider, YourDatabaseCsProvider>();

// Or use with factory pattern
services.AddSingleton<ICsProviderFactory, CsProviderFactory>();
```

## Implementation Guidelines

When implementing the interfaces from this package:

- **Thread Safety**: Ensure implementations are thread-safe
- **Configuration**: Support various configuration sources
- **Security**: Handle credentials securely
- **Error Handling**: Provide meaningful error messages
- **Logging**: Integrate with logging frameworks appropriately

## Requirements

- .NET 9.0 or higher

## Related Packages

This abstractions package works with database-specific implementation packages:

- Implementation packages for various database providers
- Extensions for specific frameworks and scenarios
- Testing utilities and mocks

## License

MIT

