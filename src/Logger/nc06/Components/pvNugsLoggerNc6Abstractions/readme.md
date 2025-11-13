# pvNugsLoggerNc6Abstractions

A comprehensive .NET 6 logging framework that provides structured, contextual logging with support for multiple output destinations, user contexts, and typed logging capabilities.

## Features

- **Multiple Output Destinations**: Support for console, SQL, and hybrid logging configurations
- **Contextual Logging**: Track user, company, and topic contexts across log entries
- **Structured Logging**: Detailed metadata for each log entry including machine name, method name, and line numbers
- **Severity Levels**: Comprehensive severity level system compatible with Microsoft's `LogLevel`
- **Method Result Tracking**: Built-in support for tracking method execution results and notifications
- **Base Implementation**: Ready-to-use abstract base class for creating custom logging implementations
- **Async Support**: Both synchronous and asynchronous logging methods
- **Unit Testing Support**: Specialized interfaces for testing logging behavior

## Installation

Install via NuGet Package Manager:

```shell
Install-Package pvNugsLoggerNc6Abstractions
```

Or via .NET CLI:

```shell
dotnet add package pvNugsLoggerNc6Abstractions
```

## Core Components

### Logger Services

- **ILoggerService**: The main logging interface that provides comprehensive logging functionality
- **BaseLoggerService**: Abstract base class implementing ILoggerService with core functionality
- **IConsoleLoggerService**: Specialized service for console output
- **IHybridLoggerService**: Combines multiple logging outputs
- **IUTestLoggerService**: Specialized service for unit testing scenarios

### Log Writers

- **ILogWriter**: Base interface for writing log entries to various destinations
- **IConsoleLogWriter**: Specialized writer for console output
- **IUTestLogWriter**: Writer with additional capabilities for testing scenarios

### Method Results

- **IMethodResult**: Tracks method execution status and notifications
- **IMethodResultNotification**: Represents individual notifications within results

### Data Structures

- **ILoggerServiceRow**: Represents a structured log entry with metadata
- **SeverityEnu**: Defines available logging severity levels
- **SqlRoleEnu**: Defines SQL database access roles

## Basic Usage

### Using Existing Implementations

```csharp
public class ExampleService
{
    private readonly ILoggerService _logger;

    public ExampleService(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task DoSomethingAsync()
    {
        // Set context for subsequent log entries
        _logger.SetUser("user123", "company456");
        _logger.SetTopic("ImportantOperation");

        try
        {
            // Log a simple message
            _logger.Log("Starting operation", SeverityEnu.Info);

            // Perform some work...

            // Log multiple messages
            await _logger.LogAsync(
                new[] { "Step 1 complete", "Step 2 complete" },
                SeverityEnu.Debug);
        }
        catch (Exception ex)
        {
            // Log exception with context
            await _logger.LogAsync(ex);
            throw;
        }
    }
}
```

### Creating Custom Implementations

```csharp
public class CustomLoggerService : BaseLoggerService
{
    public CustomLoggerService(SeverityEnu minLevel, params ILogWriter[] logWriters)
        : base(minLevel, logWriters)
    {
    }

    // BaseLoggerService provides all the core functionality
    // Override specific methods if needed for custom behavior
}

public class CustomLogWriter : ILogWriter
{
    public void WriteLog(string? userId, string? companyId, string? topic,
        SeverityEnu severity, string machineName, string memberName,
        string filePath, int lineNumber, string message, DateTime dateUtc)
    {
        // Implement your custom log writing logic
    }

    public async Task WriteLogAsync(string? userId, string? companyId, string? topic,
        SeverityEnu severity, string machineName, string memberName,
        string filePath, int lineNumber, string message, DateTime dateUtc)
    {
        // Implement your custom async log writing logic
        await Task.CompletedTask;
    }

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
```

### Unit Testing

```csharp
public class LoggingTests
{
    private readonly IUTestLogWriter _logWriter;
    private readonly IUTestLoggerService _logger;

    [Fact]
    public void ShouldLogError()
    {
        _logger.Log("Test error", SeverityEnu.Error);

        Assert.True(_logWriter.Contains("Test error"));
        var logEntry = _logWriter.FindLastMatchingRow("Test error");
        Assert.Equal(SeverityEnu.Error, logEntry.Severity);
    }
}
```

## Available Implementations

This abstractions package provides the foundation for logging. For ready-to-use implementations, consider these companion packages:

- **pvNugsLoggerNc6MsSql**: SQL Server logging implementation
- **pvNugsLoggerNc6Serilog**: Serilog integration implementation

## Microsoft.Extensions.Logging Integration

The `BaseLoggerService` implements `Microsoft.Extensions.Logging.ILogger` interface, making it fully compatible with .NET 6's built-in logging framework and dependency injection container.