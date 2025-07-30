namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines a logging service specifically targeting Microsoft SQL Server.
/// Implements the SQL logging functionality with MS SQL Server-specific
/// optimizations and features. Extends <see cref="ISqlLoggerService"/>
/// </summary>
public interface IMsSqlLoggerService : ISqlLoggerService;

/// <summary>
/// Generic version of the Microsoft SQL Server logging service that combines
/// MS SQL Server-specific features with typed logging support.
/// </summary>
/// <typeparam name="T">
/// The type that provides context for the logger.
/// </typeparam>
public interface IMsSqlLoggerService<out T>: ISqlLoggerService<T>;