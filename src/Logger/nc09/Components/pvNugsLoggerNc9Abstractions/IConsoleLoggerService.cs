namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Provides console-specific logging functionality
/// by extending the base logger service.
/// Enables console output for all logging operations
/// defined in <see cref="ILoggerService"/>.
/// </summary>
public interface IConsoleLoggerService : ILoggerService;
