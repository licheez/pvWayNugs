namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Defines a no-operation (NOP) logging service that silently discards all log entries.
/// Useful for scenarios where logging needs to be temporarily disabled or
/// in testing environments where logging is not desired.
/// </summary>
public interface IMuteLoggerService : ILoggerService{}
