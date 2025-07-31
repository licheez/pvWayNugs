# pvNugsLoggerNc9

A comprehensive .NET logging solution providing flexible, structured logging with support for multiple outputs and contextual information.

## Features

- Multiple logging destinations (Console, SQL, Hybrid) with a unified interface
- Contextual logging with user and company information
- Topic-based log organization
- Automatic caller information capture (method name, file path, line number)
- Async and sync logging methods
- Exception handling with detailed stack traces
- Severity level filtering
- Integration with Microsoft.Extensions.Logging
- Full support for dependency injection

## Installation

Install via NuGet Package Manager:
```
bash
dotnet add package pvNugsLoggerNc9
```
Or via Package Manager Console:
```
powershell
Install-Package pvNugsLoggerNc9
```
## Quick Start
```
csharp
// Register services in your DI container
services.AddConsoleLogger(); // For console logging
// or
services.AddSqlLogger(connectionString); // For SQL logging
// or
services.AddHybridLogger(); // For multiple outputs

// Inject and use in your code
public class MyService
{
private readonly ILoggerService _logger;

    public MyService(ILoggerService logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        // Set context
        _logger.SetUser("userId", "companyId");
        _logger.SetTopic("Operations");

        // Log messages
        _logger.Log("Operation started", SeverityEnu.Info);

        try
        {
            // Your code here
        }
        catch (Exception ex)
        {
            _logger.Log(ex); // Automatically includes stack trace
        }
    }
}
```
## Logger Types

### Console Logger
Outputs logs to the console with formatted messages and color-coding based on severity.

### SQL Logger
Stores logs in a SQL database with structured data for efficient querying and analysis.

### Hybrid Logger
Combines multiple logging destinations for simultaneous output to different targets.

## Advanced Features

### Topic-based Logging
```
csharp
_logger.SetTopic("UserManagement");
_logger.Log("User profile updated", SeverityEnu.Info);
```
### Async Logging
```
csharp
await _logger.LogAsync("Async operation completed", SeverityEnu.Debug);
```
### Batch Message Logging
```
csharp
var messages = new[] { "Step 1", "Step 2", "Step 3" };
_logger.Log(messages, SeverityEnu.Info);
```
### Method Result Logging
```
csharp
var result = await operation.ExecuteAsync();
_logger.Log(result); // Logs result status and any notifications
```
## Configuration

### Minimum Log Level
```
csharp
services.Configure<PvNugsLoggerConfig>(config =>
{
config.MinLogLevel = "Debug"; // Trace, Debug, Info, Warning, Error, Fatal
});
```
### Multiple Outputs
```
csharp
services.AddHybridLogger(config =>
{
config.AddConsoleOutput()
.AddSqlOutput(connectionString);
});
```
## Dependencies

- .NET 9.0+
- Microsoft.Extensions.Logging.Abstractions

## License

MIT License

## Author

Pierre Van Wallendael

## Links

- [Source Code](https://github.com/licheez/pvWayNugs)
- [Issue Tracking](https://github.com/licheez/pvWayNugs/issues)
- [Documentation](https://github.com/licheez/pvWayNugs/wiki)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
