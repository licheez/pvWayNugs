namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines a Serilog-based console logging service that implements
/// console output using Serilog's structured logging capabilities.
/// Provides enhanced formatting and structured data support
/// compared to standard console logging.
/// </summary>
public interface ISeriConsoleLoggerService : IConsoleLoggerService;

/// <summary>
/// Generic version of the Serilog console logging service that combines
/// Serilog's structured logging capabilities with typed logging support.
/// </summary>
/// <typeparam name="T">
/// The type that provides context for the logger.
/// </typeparam>
public interface ISeriConsoleLoggerService<out T>: IConsoleLoggerService<T>;