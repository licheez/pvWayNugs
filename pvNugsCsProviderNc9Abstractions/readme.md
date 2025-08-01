# pvNugsCsProviderNc9Abstractions

A .NET library providing abstractions for managing database connection strings with role-based access control, specifically designed for PostgreSQL integration.

## Features

- Role-based connection string management
- PostgreSQL-specific connection string provider
- Support for dynamic credentials
- Schema-aware configuration
- Async-first design
- .NET 9.0 target framework support

## Installation

Install the package via NuGet:
```
bash
dotnet add package pvNugsCsProviderNc9Abstractions
```
## Usage

### Basic Connection String Provider
```
csharp
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
### PostgreSQL-Specific Provider

```csharp
// Inject the PostgreSQL provider
public class MyPostgresService
{
    private readonly IPvNugsPgSqlCsProvider _pgProvider;

    public MyPostgresService(IPvNugsPgSqlCsProvider pgProvider)
    {
        _pgProvider = pgProvider;
    }

    public void ConfigureConnection()
    {
        // Access PostgreSQL-specific properties
        string schema = _pgProvider.Schema;
        string username = _pgProvider.UserName;
        bool usesDynamicCreds = _pgProvider.UseDynamicCredentials;
        SqlRoleEnu currentRole = _pgProvider.Role;
    }
}
```

## Available Roles

The package defines three levels of database access roles:

- `SqlRoleEnu.Owner` - Highest privilege level with full database control
- `SqlRoleEnu.Application` - Standard application-level privileges with write access
- `SqlRoleEnu.Reader` - Read-only access level

## Requirements

- .NET 9.0 or higher

## License

[Specify your license here]

## Contributing

[Specify contribution guidelines or link to them]
