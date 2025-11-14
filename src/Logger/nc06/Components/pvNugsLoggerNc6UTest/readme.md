# pvNugsLoggerNc6UTest

## 📋 Overview

A lightweight in-memory logging implementation for unit testing in .NET 6 applications. This library provides a test-friendly logger that captures log entries in memory, allowing you to verify logging behavior in your unit tests.

## ✨ Features

- 💾 **In-Memory Log Storage**: Captures all log entries in memory for easy verification
- 💉 **Dependency Injection Support**: Seamlessly integrates with Microsoft.Extensions.DependencyInjection
- 🔍 **Search & Query**: Find specific log entries by message content
- 📊 **Severity Levels**: Supports all standard severity levels (Trace, Debug, Info, Warning, Error, Fatal)
- 🏷️ **Metadata Capture**: Records user ID, company ID, topic, machine name, member name, file path, and line number
- ⚡ **Async/Sync Support**: Both synchronous and asynchronous logging methods
- 🧹 **Easy Cleanup**: Clear logs between tests

## 📦 Installation

```bash
dotnet add package pvNugsLoggerNc6UTest
```

## 🚀 Usage

### Basic Setup

```csharp
// Create logger service directly
var service = PvNugsLoggerUTestDi.CreateService(out IUTestLogWriter logWriter);

// Or use dependency injection
var services = new ServiceCollection();
var logWriter = services.AddPvWayUTestLoggerService();
```

### Writing Logs

```csharp
await service.LogAsync("Test message");
service.Log(SeverityEnu.Warning, "Warning message");
```

### Verifying Logs in Tests

```csharp
// Check if a log contains specific text
bool hasLog = logWriter.HasLog("Test message");

// Find specific log entries
var firstMatch = logWriter.FindFirstMatchingRow("error");
var lastMatch = logWriter.FindLastMatchingRow("warning");

// Access all logs
var allLogs = logWriter.LogRows;

// Clear logs between tests
logWriter.ClearLogs();
```

## 📚 Dependencies

- Microsoft.Extensions.DependencyInjection
- pvNugsLoggerNc6Abstractions

## 🎯 Target Framework

- .NET 6.0

## 📄 License

See LICENSE file for details.

## 👤 Author

pvWay

