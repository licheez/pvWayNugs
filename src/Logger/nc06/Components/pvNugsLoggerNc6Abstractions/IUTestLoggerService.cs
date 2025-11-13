namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Defines a specialized logging service for unit testing scenarios.
/// Extends the base <see cref="ILoggerService"/> functionality
/// to provide logging capabilities specifically designed for unit tests.
/// This enables verification and assertion of logging behavior
/// during test execution.
/// </summary>
/// <seealso cref="ILoggerService"/>
public interface IUTestLoggerService : ILoggerService{}
