namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Represents a specialized log writer for SQL database output.
/// Extends the base log writer functionality
/// to specifically target SQL databases as the storage medium.
/// </summary>
public interface ISqlLogWriter : ILogWriter
{
    /// <summary>
    /// Asynchronously purges log entries from the database based on severity and retention policies.
    /// </summary>
    /// <param name="retainDic">
    /// A dictionary mapping severity levels to their respective retention periods.
    /// Log entries older than the retention period for each severity will be deleted.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous purge operation. 
    /// The task result contains the total number of rows deleted across all severity levels.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when a database error occurs during the purge operation.
    /// The original exception is wrapped and can be accessed via the <see cref="Exception.InnerException"/> property.
    /// </exception>
    /// <remarks>
    /// <para>This method performs lazy initialization on first call, which may include table creation and validation.</para>
    /// <para>The purge operation processes each severity level sequentially within a single database connection.</para>
    /// <para>The cutoff date is calculated as <c>DateTime.UtcNow - retainPeriod</c> for each severity.</para>
    /// <para>All database operations use parameterized queries to prevent SQL injection attacks.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var retentionPolicy = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
    /// {
    ///     { SeverityEnu.Error, TimeSpan.FromDays(90) },
    ///     { SeverityEnu.Warning, TimeSpan.FromDays(30) },
    ///     { SeverityEnu.Info, TimeSpan.FromDays(7) }
    /// };
    /// 
    /// int deletedRows = await logWriter.PurgeLogsAsync(retentionPolicy);
    /// Console.WriteLine($"Purged {deletedRows} log entries");
    /// </code>
    /// </example>
    Task<int> PurgeLogsAsync(IDictionary<SeverityEnu, TimeSpan> retainDic);
}
