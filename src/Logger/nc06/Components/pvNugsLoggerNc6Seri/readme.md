# pvNugsLoggerNc6Seri

A Serilog-based console logging implementation for the pvNugsLogger framework, targeting .NET 6.0.

## Features

- Structured console logging using Serilog
- Color-coded output based on severity levels
- Contextual logging support (user ID, company ID, topics)
- Full integration with Microsoft.Extensions.Logging
- Both synchronous and asynchronous logging methods
- Comprehensive exception logging with full stack traces
- UTC timestamp formatting using invariant culture

## Installation

Install via NuGet Package Manager:
```shell
Install-Package pvNugsLoggerNc6Seri
```
Or using the .NET CLI:
```shell
dotnet add package pvNugsLoggerNc6Seri
```
## Quick Start

1. Add to your services in `Program.cs`:
```
csharp
builder.Services.AddPvNugsLoggerSeriService(builder.Configuration);
```
2. Configure in `appsettings.json`:
```json
{
  "PvNugsLogger": {
    "MinLevel": "Debug"
  }
}
```
```


3. Inject and use in your code:
```csharp
public class MyService
{
    private readonly ILoggerService _logger;

    public MyService(ILoggerService logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        _logger.Log("Operation started", SeverityEnu.Info);
        
        // With topic
        _logger.Log("Processing complete", "OrderProcessor", SeverityEnu.Debug);
        
        // With user context
        _logger.SetUser("user123", "company456");
        _logger.Log("User action performed", SeverityEnu.Info);
    }
}
```


## Dependencies

- .NET 6.0
- Microsoft.Extensions.Logging.Abstractions (6.0.0+)
- Microsoft.Extensions.Options.ConfigurationExtensions (6.0.0+)
- Serilog.Sinks.Console (4.0.0+)
- pvNugsLoggerNc6Abstractions (6.0.0+)

## License

MIT License

## Author

Pierre Van Wallendael

## Repository

[GitHub Repository](https://github.com/licheez/pvWayNugs.git)

## Support

For issues and feature requests, please use the GitHub issue tracker.
