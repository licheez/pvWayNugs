namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Provides console-specific logging functionality
/// by extending the base logger service.
/// Enables console output for all logging operations
/// defined in <see cref="ILoggerService"/>.
/// </summary>
public interface IConsoleLoggerService : ILoggerService;

/// <summary>
/// Generic version of IConsoleLoggerService that combines
/// console logging functionality with typed logging support.
/// </summary>
/// <typeparam name="T">
/// The type that provides context for the logger.
/// </typeparam>
public interface IConsoleLoggerService<out T>: ILoggerService<T>;