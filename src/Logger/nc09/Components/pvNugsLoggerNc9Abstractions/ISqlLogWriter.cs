
namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Represents a specialized log writer for SQL database output.
/// Extends the base log writer functionality to specifically target SQL databases as the storage medium.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for SQL database-specific log writers that can perform
/// advanced operations such as log purging with retention policies. Implementations should
/// provide robust, secure, and performant logging to SQL databases.
/// </para>
/// <para>
/// All SQL log writer implementations should support:
/// </para>
/// <list type="bullet">
/// <item>Parameterized queries to prevent SQL injection attacks</item>
/// <item>Thread-safe operations for concurrent access</item>
/// <item>Configurable retention policies for log management</item>
/// <item>Proper error handling and logging</item>
/// </list>
/// </remarks>
public interface ISqlLogWriter : ILogWriter
{
    /// <summary>
    /// Asynchronously purges log entries from the database based on severity and retention policies.
    /// </summary>
    /// <param name="retainDic">
    /// A dictionary mapping severity levels to their respective retention periods.
    /// Log entries older than the retention period for each severity will be deleted.
    /// If null or not provided, the implementation should use its configured default retention policies.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous purge operation. 
    /// The task result contains the total number of rows deleted across all severity levels.
    /// Returns 0 if no retention policies are configured or no records match the purge criteria.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when a database error occurs during the purge operation.
    /// Specific exception types may vary by implementation (e.g., SqlException, InvalidOperationException).
    /// The original exception details should be preserved for diagnostics.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method may perform lazy initialization on first call, which could include table creation and validation
    /// depending on the implementation's configuration.
    /// </para>
    /// <para>
    /// The purge operation typically processes each severity level sequentially within a single database connection
    /// for consistency. The cutoff date for each severity is calculated as <c>DateTime.UtcNow - retainPeriod</c>.
    /// </para>
    /// <para>
    /// All database operations should use parameterized queries to prevent SQL injection attacks.
    /// </para>
    /// <para>
    /// <strong>Default Behavior:</strong> When <paramref name="retainDic"/> is null, implementations should
    /// use their configured default retention policies. These defaults may vary by implementation but should
    /// provide sensible retention periods for different severity levels.
    /// </para>
    /// <para>
    /// <strong>Performance Consideration:</strong> For large log tables, this operation may take considerable time.
    /// Consider running during maintenance windows for optimal performance.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use default retention policies from implementation configuration
    /// int defaultDeleted = await logWriter.PurgeLogsAsync();
    /// Console.WriteLine($"Purged {defaultDeleted} log entries using default policies");
    /// 
    /// // Use custom retention policies
    /// var customRetentionPolicy = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
    /// {
    ///     { SeverityEnu.Critical, TimeSpan.FromDays(365) },
    ///     { SeverityEnu.Error, TimeSpan.FromDays(90) },
    ///     { SeverityEnu.Warning, TimeSpan.FromDays(30) },
    ///     { SeverityEnu.Info, TimeSpan.FromDays(7) },
    ///     { SeverityEnu.Debug, TimeSpan.FromDays(1) }
    /// };
    /// 
    /// int customDeleted = await logWriter.PurgeLogsAsync(customRetentionPolicy);
    /// Console.WriteLine($"Purged {customDeleted} log entries using custom policies");
    /// 
    /// // Purge only specific severity levels
    /// var errorOnlyPolicy = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
    /// {
    ///     { SeverityEnu.Error, TimeSpan.FromDays(30) }
    /// };
    /// int errorDeleted = await logWriter.PurgeLogsAsync(errorOnlyPolicy);
    /// </code>
    /// </example>
    Task<int> PurgeLogsAsync(IDictionary<SeverityEnu, TimeSpan>? retainDic = null);
}