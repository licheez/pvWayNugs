using Microsoft.Extensions.Options;
using pvNugsLoggerNc9;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9MsSql;

/// <summary>
/// Provides a Microsoft SQL Server-based logging service implementation.
/// Extends the base logging functionality with SQL Server-specific operations such as log purging.
/// </summary>
/// <remarks>
/// <para>
/// This service delegates core logging operations to the <see cref="BaseLoggerService"/> while providing
/// SQL Server-specific functionality through the injected <see cref="IMsSqlLogWriter"/>.
/// </para>
/// <para>
/// The service supports all standard logging operations including contextual logging (user/company/topic),
/// structured logging with metadata, and SQL Server-specific maintenance operations like log purging.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Configuration in DI container
/// services.AddSingleton&lt;IMsSqlLogWriter, MsSqlLogWriter&gt;();
/// services.AddSingleton&lt;IMsSqlLoggerService, MsSqlLoggerService&gt;();
/// 
/// // Usage in application
/// public class MyService
/// {
///     private readonly IMsSqlLoggerService _logger;
///     
///     public MyService(IMsSqlLoggerService logger)
///     {
///         _logger = logger;
///     }
///     
///     public async Task DoWorkAsync()
///     {
///         _logger.SetUser("user123", "company456");
///         _logger.Log("Starting work", SeverityEnu.Info);
///         
///         // Perform maintenance
///         var retentionPolicy = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
///         {
///             { SeverityEnu.Error, TimeSpan.FromDays(90) },
///             { SeverityEnu.Info, TimeSpan.FromDays(30) }
///         };
///         int purgedRows = await _logger.PurgeLogsAsync(retentionPolicy);
///     }
/// }
/// </code>
/// </example>
public class MsSqlLoggerService(
    SeverityEnu minLogLevel,
    IMsSqlLogWriter logWriter): BaseLoggerService(minLogLevel, logWriter),
    IMsSqlLoggerService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlLoggerService"/> class using configuration options.
    /// </summary>
    /// <param name="options">
    /// Configuration options containing logging settings including minimum log level.
    /// Cannot be null and must contain a valid <see cref="PvNugsLoggerConfig.MinLevel"/> value.
    /// </param>
    /// <param name="logWriter">
    /// The SQL Server log writer implementation that will handle the actual database operations.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is null or when <paramref name="options.Value"/> is null.
    /// </exception>
    /// <remarks>
    /// This constructor is primarily used in dependency injection scenarios where logging configuration
    /// is provided through the options pattern. It extracts the minimum log level from the configuration
    /// and delegates to the primary constructor.
    /// </remarks>
    public MsSqlLoggerService(
        IOptions<PvNugsLoggerConfig> options,
        IMsSqlLogWriter logWriter): this(options.Value.MinLevel, logWriter)
    {
    }

    /// <summary>
    /// Asynchronously purges log entries from the SQL Server database based on severity-specific retention policies.
    /// </summary>
    /// <param name="retainDic">
    /// A dictionary mapping <see cref="SeverityEnu"/> values to their respective retention periods.
    /// Log entries older than the specified <see cref="TimeSpan"/> for each severity level will be permanently deleted.
    /// Cannot be null or empty.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous purge operation.
    /// The task result contains the total number of log entries deleted across all processed severity levels.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="retainDic"/> is null.
    /// </exception>
    /// <exception cref="MsSqlLogWriterException">
    /// Thrown when a database error occurs during the purge operation.
    /// The original database exception can be accessed through the <see cref="Exception.InnerException"/> property.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method delegates to the underlying IMsSqlLogWriter.PurgeLogsAsync implementation,
    /// which performs the database operations using parameterized queries for security.
    /// </para>
    /// <para>
    /// The purge operation processes each severity level sequentially within a single database transaction,
    /// ensuring consistency. The cutoff date for each severity is calculated as <c>DateTime.UtcNow - retentionPeriod</c>.
    /// </para>
    /// <para>
    /// <strong>Performance Note:</strong> For large log tables, this operation may take considerable time.
    /// Consider running during maintenance windows or implementing batched deletion strategies for very large datasets.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Define retention policies by severity
    /// var retentionPolicies = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
    /// {
    ///     { SeverityEnu.Critical, TimeSpan.FromDays(365) },  // Keep critical logs for 1 year
    ///     { SeverityEnu.Error, TimeSpan.FromDays(90) },      // Keep errors for 90 days
    ///     { SeverityEnu.Warning, TimeSpan.FromDays(30) },    // Keep warnings for 30 days
    ///     { SeverityEnu.Info, TimeSpan.FromDays(7) },        // Keep info logs for 7 days
    ///     { SeverityEnu.Debug, TimeSpan.FromDays(1) }        // Keep debug logs for 1 day
    /// };
    /// 
    /// try
    /// {
    ///     int totalDeleted = await loggerService.PurgeLogsAsync(retentionPolicies);
    ///     Console.WriteLine($"Successfully purged {totalDeleted} log entries");
    /// }
    /// catch (MsSqlLogWriterException ex)
    /// {
    ///     Console.WriteLine($"Failed to purge logs: {ex.Message}");
    ///     // Handle database-specific errors
    /// }
    /// </code>
    /// </example>
    public async Task<int> PurgeLogsAsync(IDictionary<SeverityEnu, TimeSpan> retainDic)
    {
        ArgumentNullException.ThrowIfNull(retainDic);
        return await logWriter.PurgeLogsAsync(retainDic);
    }
}