namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines a logging service specifically focusing on Postgres.
/// Implements the SQL logging functionality with Postgres-specific
/// optimizations and features.
/// </summary>
public interface IPgSqlLoggerService : ISqlLoggerService;

/// <summary>
/// Generic version of the PostgresSQL logging service that combines
/// Postgres-specific features with typed logging support.
/// </summary>
/// <typeparam name="T">
/// The type that provides context for the logger.
/// </typeparam>
public interface IPgSqlLoggerService<out T>: ISqlLoggerService<T>;