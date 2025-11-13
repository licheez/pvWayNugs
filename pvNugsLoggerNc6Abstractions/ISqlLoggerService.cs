namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Defines a logging service that persists log entries to a SQL database.
/// Extends the base logger service functionality with SQL-specific operations such as log retention management.
/// </summary>
/// <remarks>
/// <para>
/// This interface represents a specialized logging service that combines the full functionality of
/// <see cref="ILoggerService"/> with SQL database-specific capabilities. It provides both standard
/// logging operations (writing, contextual tracking) and advanced database maintenance features.
/// </para>
/// <para>
/// Implementations of this interface should provide:
/// </para>
/// <list type="bullet">
/// <item>All standard logging capabilities from the base <see cref="ILoggerService"/></item>
/// <item>SQL database-specific log storage with proper indexing and performance optimization</item>
/// <item>Configurable retention policies for automated log management</item>
/// <item>Transaction safety and error handling for database operations</item>
/// <item>Support for high-volume logging scenarios</item>
/// </list>
/// <para>
/// This service is ideal for enterprise applications requiring persistent, queryable log storage
/// with automated maintenance capabilities.
/// </para>
/// </remarks>
public interface ISqlLoggerService: ILoggerService
{
    /// <summary>
    /// Asynchronously removes log entries from the database based on their age and severity level.
    /// </summary>
    /// <param name="retainDic">
    /// A dictionary mapping <see cref="SeverityEnu"/> values to their respective retention periods as <see cref="TimeSpan"/> values.
    /// Log entries older than their specified retention period will be permanently purged from the database.
    /// If null or not provided, the implementation should use its configured default retention policies.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous purge operation.
    /// The task result contains the total number of log entries that were successfully purged from the database.
    /// Returns 0 if no retention policies are configured, no records match the purge criteria, or if the database is empty.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the logging service is not properly configured or when database connectivity issues prevent the operation.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the database user lacks sufficient permissions to perform DELETE operations on the log table.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method provides automated log maintenance by removing old entries based on configurable retention policies.
    /// It's designed to be run periodically (e.g., daily, weekly) to prevent log tables from growing indefinitely.
    /// </para>
    /// <para>
    /// <strong>Default Behavior:</strong> When <paramref name="retainDic"/> is null, the implementation should
    /// use default retention policies configured during service setup. These defaults typically provide
    /// longer retention for critical/error logs and shorter retention for informational/debug logs.
    /// </para>
    /// <para>
    /// <strong>Operation Details:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>The cutoff date for each severity is calculated as <c>DateTime.UtcNow - retentionPeriod</c></item>
    /// <item>Only log entries older than the cutoff date for their respective severity level are removed</item>
    /// <item>The operation is performed within database transactions to ensure data consistency</item>
    /// <item>Progress and results are typically logged for audit and monitoring purposes</item>
    /// </list>
    /// <para>
    /// <strong>Performance Considerations:</strong> For tables with millions of log entries, this operation
    /// may take significant time and resources. Consider scheduling during low-usage periods and monitoring
    /// database performance during execution.
    /// </para>
    /// <para>
    /// <strong>Safety:</strong> This operation permanently deletes data. Ensure you have appropriate
    /// backups and that retention policies align with your compliance and operational requirements.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use default retention policies configured in the service
    /// var logger = serviceProvider.GetRequiredService&lt;ISqlLoggerService&gt;();
    /// int purgedCount = await logger.PurgeLogsAsync();
    /// Console.WriteLine($"Purged {purgedCount} log entries using default policies");
    /// 
    /// // Use custom retention policies for specific business needs
    /// var customRetentionPolicies = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
    /// {
    ///     { SeverityEnu.Critical, TimeSpan.FromDays(730) },  // 2 years for critical issues
    ///     { SeverityEnu.Error, TimeSpan.FromDays(180) },     // 6 months for errors
    ///     { SeverityEnu.Warning, TimeSpan.FromDays(60) },    // 2 months for warnings
    ///     { SeverityEnu.Info, TimeSpan.FromDays(14) },       // 2 weeks for info logs
    ///     { SeverityEnu.Debug, TimeSpan.FromDays(3) }        // 3 days for debug logs
    /// };
    /// 
    /// int customPurgedCount = await logger.PurgeLogsAsync(customRetentionPolicies);
    /// Console.WriteLine($"Custom purge removed {customPurgedCount} entries");
    /// 
    /// // Selective purging - only remove debug and info logs
    /// var selectivePurge = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
    /// {
    ///     { SeverityEnu.Debug, TimeSpan.FromHours(24) },
    ///     { SeverityEnu.Info, TimeSpan.FromDays(7) }
    /// };
    /// 
    /// int selectivePurgedCount = await logger.PurgeLogsAsync(selectivePurge);
    /// Console.WriteLine($"Selective purge removed {selectivePurgedCount} debug/info entries");
    /// 
    /// // Scheduled maintenance example
    /// public class LogMaintenanceService : BackgroundService
    /// {
    ///     private readonly ISqlLoggerService _logger;
    ///     
    ///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    ///     {
    ///         while (!stoppingToken.IsCancellationRequested)
    ///         {
    ///             try
    ///             {
    ///                 // Run daily maintenance at 2 AM
    ///                 var now = DateTime.Now;
    ///                 var next = now.Date.AddDays(1).AddHours(2);
    ///                 var delay = next - now;
    ///                 
    ///                 await Task.Delay(delay, stoppingToken);
    ///                 
    ///                 int purged = await _logger.PurgeLogsAsync();
    ///                 // Log the maintenance operation result
    ///                 await _logger.LogAsync($"Daily log maintenance completed: {purged} entries purged", 
    ///                                       SeverityEnu.Info);
    ///             }
    ///             catch (Exception ex)
    ///             {
    ///                 await _logger.LogAsync(ex);
    ///                 await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Retry in 1 hour
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<int> PurgeLogsAsync(IDictionary<SeverityEnu, TimeSpan>? retainDic = null);
}