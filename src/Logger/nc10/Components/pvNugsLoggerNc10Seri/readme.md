# pvNugsLoggerNc10Seri

A Serilog-based console logging implementation for the pvNugsLogger framework, targeting .NET 10.

## Overview

This component integrates a console logger with the standard Microsoft logging abstractions using a custom `ILoggerFactory` and a console-specific writer implementation.

It registers:

- `ILogWriter` and `IConsoleLogWriter`
- `ILoggerFactory` => `SeriLogLoggerFactory`
- `ISeriConsoleLoggerService`
- `IConsoleLoggerService`
- `ILoggerService`

The logger follows the `TryAdd...` registration pattern so repeated startup calls do not create duplicate singleton registrations.

## Features

- Serilog console output
- Severity filtering via `PvNugsLoggerConfig.MinLogLevel`
- Integration with `Microsoft.Extensions.Logging.ILoggerFactory`
- Support for standard `ILogger` usage patterns
- Context-aware logging via user/company/topic fields
- Synchronous and asynchronous log APIs
- Structured exception logging and stack trace handling
- Compatible with the pvNugs logger abstraction layer

## Requirements

- .NET 10.0 or later
- C# 13 or later
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.Options.ConfigurationExtensions`
- Serilog console sink dependency

## Quick start

1. Register the logging services in `Program.cs`:

```csharp
builder.Services.TryAddPvNugsLoggerSeriService(builder.Configuration);
```

2. Configure the logger in `appsettings.json`:

```json
{
  "PvNugsLoggerConfig": {
    "MinLogLevel": "Debug"
  }
}
```

3. Inject and use the logger:

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
        _logger.SetUser("user123", "company456");
        _logger.Log("Operation started", SeverityEnu.Info);
        _logger.Log("Processing complete", "OrderProcessor", SeverityEnu.Debug);
    }
}
```

## Registration details

The DI extension method is:

```csharp
services.TryAddPvNugsLoggerSeriService(configuration);
```

This method configures the writer and logger factory using the `PvNugsLoggerConfig` section and registers the following services as singletons:

- `ILogWriter`
- `IConsoleLogWriter`
- `ILoggerFactory`
- `ISeriConsoleLoggerService`
- `IConsoleLoggerService`
- `ILoggerService`

## Notes

- The factory class is `SeriLogLoggerFactory` and is responsible for creating category-specific loggers.
- The writer class is `SerilogConsoleWriter`.
- The logger implementation is `SerilogConsoleService`.
- The provider implementation is `SerilogConsoleLoggerProvider` for provider-based logging scenarios.

## Dependencies

- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.Options.ConfigurationExtensions`
- `pvNugsLoggerNc10Abstractions`
- Serilog console sink package

## License

MIT License

## Repository

[GitHub Repository](https://github.com/licheez/pvWayNugs.git)
