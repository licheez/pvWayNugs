namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines a specialized logging service for unit testing scenarios.
/// Extends the base <see cref="ILoggerService"/> functionality
/// to provide logging capabilities specifically designed for unit tests.
/// This enables verification and assertion of logging behavior
/// during test execution.
/// </summary>
/// <seealso cref="ILoggerService"/>
public interface IUTestLoggerService : ILoggerService;

/// <summary>
/// Generic version of the unit test logging service that combines
/// unit test logging capabilities with typed logging support.
/// See also <see cref="IUTestLoggerService"/>
/// </summary>
/// <typeparam name="T">
/// The type that provides context for the logger.
/// </typeparam>
public interface IUTestLoggerService<out T>: ILoggerService<T>;