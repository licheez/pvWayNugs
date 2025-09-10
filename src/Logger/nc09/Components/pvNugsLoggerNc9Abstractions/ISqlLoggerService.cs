namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines a logging service that persists log entries to a SQL database.
/// Extends the base logger service functionality with SQL-specific
/// operations such as log retention management.
/// </summary>
public interface ISqlLoggerService: ILoggerService
{
    /// <summary>
    /// Asynchronously removes log entries based on their age and severity level.
    /// </summary>
    /// <param name="retainDic">
    /// A dictionary mapping severity levels to their respective retention periods.
    /// Log entries older than their specified retention period will be purged.
    /// </param>
    /// <returns>
    /// The number of log entries that were purged from the database.
    /// </returns>
    Task<int> PurgeLogsAsync(IDictionary<SeverityEnu, TimeSpan> retainDic);
}
