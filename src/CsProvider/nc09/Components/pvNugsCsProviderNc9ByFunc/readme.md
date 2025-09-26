# pvNugsCsProviderNc9ByFunc

A flexible, function-based connection string provider implementation for .NET 9.0 applications.

## Features

- Function-based connection string retrieval with SQL role support
- **Single- or Multi-Database** support via DI overloads
- Dependency injection integration
- Built-in error handling with custom exception types
- Async-first design
- Console logging integration

## Installation

Install via NuGet Package Manager:
```powershell
Install-Package pvNugsCsProviderNc9ByFunc
```
Or via .NET CLI:
```bash
dotnet add package pvNugsCsProviderNc9ByFunc
```

## Dependencies

- .NET 9.0
- pvNugsCsProviderNc9Abstractions
- pvNugsLoggerNc9Abstractions

## Usage

### 1. Register the service in your dependency injection container

#### Single-Database (default)
```csharp
services.AddPvNugsCsProvider(async (role, ct) => {
    // Your connection string retrieval logic here
    return "your-connection-string";
});
```

#### Multi-Database
```csharp
services.AddPvNugsCsProvider(async (connectionStringName, role, ct) => {
    // Your logic to select the connection string based on name and role
    if (connectionStringName == "MainDb")
        return "main-db-connection-string";
    if (connectionStringName == "AuditDb")
        return "audit-db-connection-string";
    throw new ArgumentException($"Unknown db: {connectionStringName}");
});
```

### 2. Inject and use the provider
```csharp
public class YourClass
{
    private readonly IPvNugsCsProvider _csProvider;

    public YourClass(IPvNugsCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    public async Task DoSomething()
    {
        // Single-db usage (or default db in multi-db)
        var connectionString = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
        // Multi-db usage
        var auditConnectionString = await _csProvider.GetConnectionStringAsync("AuditDb", SqlRoleEnu.Reader);
        // Use the connection string(s)
    }
}
```

## Features in Detail

### Function-Based Provider
The provider accepts a function that handles the actual connection string retrieval, allowing for flexible implementation of your storage and retrieval logic.

### Single- or Multi-Database Support
- **Single-db:** Use `Func<SqlRoleEnu, CancellationToken, Task<string>>` for simple scenarios.
- **Multi-db:** Use `Func<string, SqlRoleEnu, CancellationToken, Task<string>>` to support multiple named database configurations.
- The correct overload of `AddPvNugsCsProvider` will be used automatically based on your function signature.

### Role-Based Access
Supports different connection strings based on SQL roles (Owner, Application, Reader).

### Error Handling
Includes built-in error handling with custom `PvNugsCsProviderException` and integrated logging.

## Interface Overloads

The provider implements both overloads of the main interface:
```csharp
Task<string> GetConnectionStringAsync(SqlRoleEnu role = SqlRoleEnu.Reader, CancellationToken cancellationToken = default);
Task<string> GetConnectionStringAsync(string connectionStringName, SqlRoleEnu role = SqlRoleEnu.Reader, CancellationToken cancellationToken = default);
```

## License

This project is licensed under the MIT License.

## Author

Pierre Van Wallendael

## Repository

[GitHub Repository](https://github.com/licheez/pvWayNugs)

## See Also
- [pvNugsCsProviderNc9Abstractions](https://www.nuget.org/packages/pvNugsCsProviderNc9Abstractions)

## Version History

### 9.0.0
- Initial release
- Support for .NET 9.0
- Function-based connection string provider implementation
- Multi-database support via DI overloads
