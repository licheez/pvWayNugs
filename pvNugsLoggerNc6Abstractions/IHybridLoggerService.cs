namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Defines a logging service that combines multiple logging outputs.
/// Enables logging to multiple destinations simultaneously while
/// maintaining the standard <see cref="ILoggerService"/> functionality.
/// </summary>
public interface IHybridLoggerService : ILoggerService{}
