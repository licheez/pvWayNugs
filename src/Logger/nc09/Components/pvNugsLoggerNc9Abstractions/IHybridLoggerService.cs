namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines a logging service that combines multiple logging outputs.
/// Enables logging to multiple destinations simultaneously while
/// maintaining the standard <see cref="ILoggerService"/> functionality.
/// </summary>
public interface IHybridLoggerService : ILoggerService;

/// <summary>
/// Generic version of the hybrid logging service that combines
/// multiple logging outputs with typed logging support.
/// See also <see cref="IHybridLoggerService"/>
/// </summary>
/// <typeparam name="T">
/// The type that provides context for the logger.
/// </typeparam>
public interface IHybridLoggerService<out T>: ILoggerService<T>;