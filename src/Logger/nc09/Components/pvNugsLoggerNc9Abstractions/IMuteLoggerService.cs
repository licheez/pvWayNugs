namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines a no-operation (NOP) logging service that silently discards all log entries.
/// Useful for scenarios where logging needs to be temporarily disabled or
/// in testing environments where logging is not desired.
/// </summary>
public interface IMuteLoggerService : ILoggerService;

/// <summary>
/// Generic version of the mute logging service that combines
/// the no-operation logging behavior with typed logging support.
/// </summary>
/// <typeparam name="T">
/// The type that provides context for the logger.
/// </typeparam>
public interface IMuteLoggerService<out T>: ILoggerService<T>;