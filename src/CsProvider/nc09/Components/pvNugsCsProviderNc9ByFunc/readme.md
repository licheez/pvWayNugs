# pvNugsCsProviderNc9ByFunc

A flexible, function-based connection string provider implementation for .NET 9.0 applications.

## Features

- Function-based connection string retrieval with SQL role support
- Dependency injection integration
- Built-in error handling with custom exception types
- Async-first design
- Console logging integration

## Installation

Install via NuGet Package Manager:
```
bash
Install-Package pvNugsCsProviderNc9ByFunc
```
Or via .NET CLI:
```
bash
dotnet add package pvNugsCsProviderNc9ByFunc
```
## Dependencies

- .NET 9.0
- pvNugsCsProviderNc9Abstractions
- pvNugsLoggerNc9Abstractions

## Usage

1. Register the service in your dependency injection container:
```
csharp
services.AddPvNugsCsProvider(async role => {
// Your connection string retrieval logic here
return "your-connection-string";
});
```
2. Inject and use the provider:
```
csharp
public class YourClass
{
private readonly IPvNugsCsProvider _csProvider;

    public YourClass(IPvNugsCsProvider csProvider)
    {
        _csProvider = csProvider;
    }

    public async Task DoSomething()
    {
        var connectionString = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
        // Use the connection string
    }
}
```
## Features in Detail

### Function-Based Provider
The provider accepts a function that handles the actual connection string retrieval, allowing for flexible implementation of your storage and retrieval logic.

### Role-Based Access
Supports different connection strings based on SQL roles (Owner, Application, Reader).

### Error Handling
Includes built-in error handling with custom `PvNugsCsProviderException` and integrated logging.

## License

This project is licensed under the MIT License.

## Author

Pierre Van Wallendael

## Repository

[GitHub Repository](https://github.com/licheez/pvWayNugs)

## Version History

### 9.0.0
- Initial release
- Support for .NET 9.0
- Function-based connection string provider implementation
