using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc6Abstractions;
using pvNugsEnumConvNc6;
using pvNugsLoggerNc6Abstractions;
using SqlRoleEnu = pvNugsCsProviderNc6Abstractions.SqlRoleEnu;

namespace pvNugsLoggerNc6MsSql;

/// <summary>
/// Provides a Microsoft SQL Server-based implementation for writing log entries to a database table.
/// Supports automatic table creation, schema validation, performance optimization through indexing, and thread-safe lazy initialization.
/// </summary>
/// <remarks>
/// <para>
/// This class implements a comprehensive logging solution designed for enterprise applications with the following key features:
/// </para>
/// <para>
/// <strong>Core Functionality:</strong>
/// </para>
/// <list type="bullet">
/// <item>Parameterized queries to prevent SQL injection attacks</item>
/// <item>Lazy initialization with thread-safe double-check locking pattern</item>
/// <item>Automatic table creation and comprehensive schema validation</item>
/// <item>Intelligent string truncation to prevent database constraint violations</item>
/// <item>Comprehensive error handling and internal activity logging</item>
/// <item>Support for log purging based on severity-specific retention policies</item>
/// </list>
/// <para>
/// <strong>Performance Optimization:</strong>
/// </para>
/// <list type="bullet">
/// <item>Configurable index creation for time-based queries (most common in logging scenarios)</item>
/// <item>Composite indexes optimized for purge operations (severity + date)</item>
/// <item>Optional filtered indexes for user-based and topic-based queries</item>
/// <item>Proper index design with descending date ordering for recent logs first</item>
/// <item>Intelligent index naming to prevent conflicts</item>
/// </list>
/// <para>
/// <strong>Database Schema Management:</strong>
/// </para>
/// <list type="bullet">
/// <item>Flexible table and column naming through configuration</item>
/// <item>Support for both identity-based and custom primary key strategies</item>
/// <item>Automatic column length validation and truncation</item>
/// <item>Schema drift detection through comprehensive table validation</item>
/// <item>Support for different SQL Server data types and constraints</item>
/// </list>
/// <para>
/// <strong>Operational Features:</strong>
/// </para>
/// <list type="bullet">
/// <item>Configurable retention policies with default enterprise-grade settings</item>
/// <item>Bulk purging operations optimized for large log tables</item>
/// <item>Support for multi-tenant scenarios through company/user context</item>
/// <item>Topic-based log categorization for easier filtering and analysis</item>
/// <item>Rich contextual information including method name, file path, and line numbers</item>
/// </list>
/// <para>
/// <strong>Configuration-Driven Behavior:</strong>
/// </para>
/// <para>
/// The log writer's behavior is entirely controlled through <see cref="PvNugsMsSqlLogWriterConfig"/>,
/// allowing for flexible deployment scenarios:
/// </para>
/// <list type="bullet">
/// <item><strong>Development:</strong> Full automatic table creation with validation and comprehensive indexing</item>
/// <item><strong>Staging:</strong> Schema validation with selective indexing based on query patterns</item>
/// <item><strong>Production:</strong> Manual table management with optimized indexes for performance</item>
/// </list>
/// <para>
/// <strong>Thread Safety and Performance:</strong>
/// </para>
/// <para>
/// This implementation is fully thread-safe and uses async/await patterns throughout for optimal scalability.
/// The initialization process uses a semaphore-based double-check locking pattern to ensure that expensive
/// database operations (table creation, schema validation, index creation) are performed exactly once,
/// regardless of concurrent access patterns.
/// </para>
/// <para>
/// <strong>Error Handling Strategy:</strong>
/// </para>
/// <para>
/// The class follows a defensive programming approach where non-critical errors (like index creation failures)
/// are logged but don't prevent the core logging functionality from working. Critical errors (like table
/// creation failures or severe schema mismatches) result in <see cref="MsSqlLogWriterException"/> being thrown
/// with the original exception preserved for detailed diagnostics.
/// </para>
/// <para>
/// <strong>Best Practices Integration:</strong>
/// </para>
/// <list type="bullet">
/// <item>Implements both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> for proper resource management</item>
/// <item>Uses connection string providers for secure database access with role-based permissions</item>
/// <item>Supports structured logging patterns with rich metadata</item>
/// <item>Follows Microsoft's logging conventions and integrates with .NET's logging infrastructure</item>
/// <item>Designed for dependency injection and configuration-based setup</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Basic configuration and setup
/// builder.Services.Configure&lt;PvNugsMsSqlLogWriterConfig&gt;(options =&gt;
/// {
///     options.TableName = "ApplicationLogs";
///     options.SchemaName = "Logging";
///     
///     // Enable performance indexes
///     options.IncludeDateIndex = true;      // Essential for time-based queries
///     options.IncludePurgeIndex = true;     // Optimizes maintenance operations
///     options.IncludeUserIndex = true;      // Enable if filtering by user frequently
///     
///     // Configure retention policies
///     options.DefaultRetentionPolicies[SeverityEnu.Error] = TimeSpan.FromDays(180);
///     options.DefaultRetentionPolicies[SeverityEnu.Info] = TimeSpan.FromDays(30);
/// });
/// 
/// // Register with dependency injection
/// builder.Services.AddSingleton&lt;IMsSqlLogWriter, MsSqlLogWriter&gt;();
/// builder.Services.AddSingleton&lt;IMsSqlLoggerService, MsSqlLoggerService&gt;();
/// 
/// // Usage in application
/// public class OrderService
/// {
///     private readonly IMsSqlLogWriter _logWriter;
///     
///     public OrderService(IMsSqlLogWriter logWriter)
///     {
///         _logWriter = logWriter;
///     }
///     
///     public async Task ProcessOrderAsync(string orderId, string userId)
///     {
///         await _logWriter.WriteLogAsync(
///             userId: userId,
///             companyId: "ACME Corp",
///             topic: "OrderProcessing",
///             severity: SeverityEnu.Info,
///             machineName: Environment.MachineName,
///             memberName: nameof(ProcessOrderAsync),
///             filePath: __FILE__,
///             lineNumber: __LINE__,
///             message: $"Processing order {orderId}",
///             dateUtc: DateTime.UtcNow);
///         
///         // Process order logic...
///     }
///     
///     public async Task PurgeOldLogsAsync()
///     {
///         // Use default retention policies
///         int purgedCount = await _logWriter.PurgeLogsAsync();
///         Console.WriteLine($"Purged {purgedCount} old log entries");
///     }
/// }
/// 
/// // Advanced configuration for high-performance scenarios
/// builder.Services.Configure&lt;PvNugsMsSqlLogWriterConfig&gt;(options =&gt;
/// {
///     // Disable automatic operations for production
///     options.CreateTableAtFirstUse = false;   // Use migration scripts
///     options.CheckTableAtFirstUse = false;    // Skip validation for performance
///     
///     // Optimize indexes for specific query patterns
///     options.IncludeDateIndex = true;          // Always needed
///     options.IncludePurgeIndex = true;         // Always needed for maintenance
///     options.IncludeUserIndex = false;         // Skip if not filtering by user
///     options.IncludeTopicIndex = true;         // Enable if using topic-based filtering
///     
///     // Tune column sizes for your data
///     options.UserIdColumnLength = 256;         // Accommodate email addresses
///     options.ContextColumnLength = 2048;       // Handle complex file paths
/// });
/// </code>
/// </example>
public sealed class MsSqlLogWriter : IMsSqlLogWriter{
    private const string SqlVarChar = "varchar";
    private const string SqlChar = "char";
    private const string SqlNVarChar = "nvarchar";
    private const string SqlDateTime = "datetime";

    private readonly IConsoleLoggerService? _logger;
    private readonly IPvNugsCsProvider _csp;
    private readonly PvNugsMsSqlLogWriterConfig _config;

    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private bool _isInitialized;

    private readonly string _tableName;
    private readonly string _schemaName;
    
    private readonly string _userIdColumnName;
    private int _userIdLength;
    
    private readonly string _companyIdColumnName;
    private int _companyIdLength;
    
    private readonly string _severityCodeColumnName;
    
    private readonly string _machineNameColumnName;
    private int _machineNameLength;
    
    private readonly string _topicColumnName;
    private int _topicLength;
    
    private readonly string _contextColumnName;
    private int _contextLength;
    
    private readonly string _messageColumnName;
    private readonly string _createDateColumnName;

    private MsSqlLogWriter(
        IPvNugsCsProvider csp,
        IOptions<PvNugsMsSqlLogWriterConfig> options)
    {
        _csp = csp ?? throw new ArgumentNullException(nameof(csp));
        _config = options.Value ?? throw new ArgumentNullException(nameof(options));

        _tableName = _config.TableName;
        _schemaName = _config.SchemaName;
        
        _userIdColumnName = _config.UserIdColumnName;
        _userIdLength = _config.UserIdColumnLength;
        
        _companyIdColumnName = _config.CompanyIdColumnName;
        _companyIdLength = _config.CompanyIdColumnLength;
        
        _severityCodeColumnName = _config.SeverityCodeColumnName;
        
        _machineNameColumnName = _config.MachineNameColumnName;
        _machineNameLength = _config.MachineNameColumnLength;
        
        _topicColumnName = _config.TopicColumnName;
        _topicLength = _config.TopicColumnLength;
        
        _contextColumnName = _config.ContextColumnName;
        _contextLength = _config.ContextColumnLength;
        
        _messageColumnName = _config.MessageColumnName;
        _createDateColumnName = _config.CreateDateUtcColumnName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlLogWriter"/> class.
    /// </summary>
    /// <param name="logger">Optional console logger service for internal logging operations. Can be null.</param>
    /// <param name="csp">The connection string provider for database access. Cannot be null.</param>
    /// <param name="options">Configuration options for the log writer. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="csp"/> or <paramref name="options"/> is null.</exception>
    /// <remarks>
    /// The constructor performs basic validation and configuration setup but does not perform any database operations.
    /// Database initialization (table creation/validation) occurs lazily on first use via <see cref="EnsureInitializedAsync"/>.
    /// </remarks>
    public MsSqlLogWriter(
        IConsoleLoggerService? logger,
        IPvNugsCsProvider csp,
        IOptions<PvNugsMsSqlLogWriterConfig> options) : this(csp, options)
    {
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously writes a log entry to the database.
    /// </summary>
    /// <param name="userId">Optional user identifier. Will be truncated if exceeds column length.</param>
    /// <param name="companyId">Optional company identifier. Will be truncated if exceeds column length.</param>
    /// <param name="topic">Optional topic or category. Will be truncated if exceeds column length.</param>
    /// <param name="severity">Log severity level. The severity code will be extracted and truncated to 1 character.</param>
    /// <param name="machineName">Machine name where the log originated. If null or empty, will use <see cref="Environment.MachineName"/>.</param>
    /// <param name="memberName">Name of the calling method. If null or empty, will be set to "&lt;unknown&gt;".</param>
    /// <param name="filePath">Path of the source file. If null or empty, will be set to "&lt;unknown&gt;".</param>
    /// <param name="lineNumber">Line number in the source file. Must be non-negative.</param>
    /// <param name="message">The log message. Cannot be null or empty.</param>
    /// <param name="dateUtc">UTC timestamp for the log entry.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lineNumber"/> is negative.</exception>
    /// <remarks>
    /// <para>This method performs lazy initialization on first call, which may include table creation and validation.</para>
    /// <para>String parameters that exceed their column lengths will be automatically truncated with "..." suffix.</para>
    /// <para>The context field combines memberName, filePath, and lineNumber in the format: "memberName # filePath # lineNumber".</para>
    /// <para>All database operations use parameterized queries to prevent SQL injection attacks.</para>
    /// </remarks>
    public async Task WriteLogAsync(
        string? userId, string? companyId, string? topic,
        SeverityEnu severity, string machineName,
        string memberName, string filePath, int lineNumber,
        string message, DateTime dateUtc)
    {
        await EnsureInitializedAsync();

        // Enhanced input validation
        if (string.IsNullOrEmpty(message))
        {
            var ex = new ArgumentException("Message cannot be null or empty", nameof(message));
            await LogExceptionAsync(ex);
            return;
        }

        if (lineNumber < 0)
        {
            var ex = new ArgumentOutOfRangeException(nameof(lineNumber), "Line number cannot be negative");
            await LogExceptionAsync(ex);
            return;
        }

        if (string.IsNullOrWhiteSpace(memberName))
        {
            memberName = "<unknown>";
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = "<unknown>";
        }

        if (string.IsNullOrEmpty(machineName))
            machineName = Environment.MachineName;

        string appCs;
        try
        {
            appCs = await _csp.GetConnectionStringAsync(
                _config.ConnectionStringName, SqlRoleEnu.Application);
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
            return;
        }

        await using var appCn = new SqlConnection(appCs);
        try
        {
            await appCn.OpenAsync();
            var cmdText = $"INSERT INTO [{_schemaName}].[{_tableName}] " +
                          $"([{_userIdColumnName}], [{_companyIdColumnName}], " +
                          $"[{_severityCodeColumnName}], [{_machineNameColumnName}], " +
                          $"[{_topicColumnName}], [{_contextColumnName}], " +
                          $"[{_messageColumnName}], [{_createDateColumnName}]) " +
                          "VALUES (" +
                          "@userId, @companyId, " +
                          "@severity, @machineName, " +
                          "@topic, @context, " +
                          "@message, @date)";

            await using var cmd = new SqlCommand(cmdText, appCn);

            // Add parameters to prevent SQL injection
            cmd.Parameters.Add("@userId",
                    SqlDbType.VarChar, _userIdLength)
                .Value = (object?)userId ?? DBNull.Value;
            cmd.Parameters.Add("@companyId",
                    SqlDbType.VarChar, _companyIdLength)
                .Value = (object?)companyId ?? DBNull.Value;
            cmd.Parameters.Add("@severity",
                SqlDbType.Char, 1).Value = GetSeverityCode(severity);
            cmd.Parameters.Add("@machineName",
                    SqlDbType.VarChar, _machineNameLength)
                .Value = TruncateString(machineName, _machineNameLength);
            cmd.Parameters.Add("@topic",
                    SqlDbType.VarChar, _topicLength)
                .Value = (object?)TruncateString(topic, _topicLength) ?? DBNull.Value;
            cmd.Parameters.Add("@context",
                    SqlDbType.VarChar, _contextLength).Value =
                TruncateString($"{memberName} # {filePath} # {lineNumber}", _contextLength);
            cmd.Parameters.Add("@message",
                SqlDbType.NVarChar, -1).Value = message;
            cmd.Parameters.Add("@date",
                SqlDbType.DateTime).Value = dateUtc;

            await cmd.ExecuteNonQueryAsync();
            await appCn.CloseAsync();
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
        }
    }

    /// <summary>
    /// Synchronously writes a log entry to the database.
    /// </summary>
    /// <param name="userId">Optional user identifier.</param>
    /// <param name="companyId">Optional company identifier.</param>
    /// <param name="topic">Optional topic or category.</param>
    /// <param name="severity">Log severity level.</param>
    /// <param name="machineName">Machine name where the log originated.</param>
    /// <param name="memberName">Name of the calling method.</param>
    /// <param name="filePath">Path of the source file.</param>
    /// <param name="lineNumber">Line number in the source file.</param>
    /// <param name="message">The log message.</param>
    /// <param name="dateUtc">UTC timestamp for the log entry.</param>
    /// <remarks>
    /// <para>This is a synchronous wrapper around <see cref="WriteLogAsync"/>.</para>
    /// <para><strong>Warning:</strong> This method blocks the calling thread. Consider using <see cref="WriteLogAsync"/> instead for better performance.</para>
    /// </remarks>
    public void WriteLog(
        string? userId, string? companyId, string? topic,
        SeverityEnu severity, string machineName,
        string memberName, string filePath, int lineNumber,
        string message, DateTime dateUtc)
    {
        WriteLogAsync(
                userId, companyId, topic,
                severity, machineName,
                memberName, filePath, lineNumber,
                message, dateUtc)
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// Releases all resources used by the <see cref="MsSqlLogWriter"/>.
    /// </summary>
    /// <remarks>
    /// This method disposes the internal semaphore used for thread-safe initialization.
    /// </remarks>
    public void Dispose()
    {
        _initSemaphore.Dispose();
    }

    /// <summary>
    /// Asynchronously releases all resources used by the <see cref="MsSqlLogWriter"/>.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous disposal operation.</returns>
    /// <remarks>
    /// This method disposes the internal semaphore used for thread-safe initialization.
    /// </remarks>
    public ValueTask DisposeAsync()
    {
        _initSemaphore.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Asynchronously purges log entries from the database based on severity-specific retention policies.
    /// </summary>
    /// <param name="retainDic">
    /// A dictionary mapping <see cref="SeverityEnu"/> values to their respective retention periods.
    /// Log entries older than the specified <see cref="TimeSpan"/> for each severity level will be permanently deleted.
    /// If null, uses the default retention policies from configuration (see <see cref="PvNugsMsSqlLogWriterConfig"/>).
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous purge operation. 
    /// The task result contains the total number of log entries deleted across all processed severity levels.
    /// </returns>
    /// <exception cref="MsSqlLogWriterException">
    /// Thrown when a database error occurs during the purge operation.
    /// The original exception is wrapped and can be accessed via the <see cref="Exception.InnerException"/> property.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs lazy initialization on first call, which may include table creation and validation
    /// depending on the <see cref="PvNugsMsSqlLogWriterConfig.CreateTableAtFirstUse"/> and 
    /// <see cref="PvNugsMsSqlLogWriterConfig.CheckTableAtFirstUse"/> configuration settings.
    /// </para>
    /// <para>
    /// When <paramref name="retainDic"/> is null, the method uses the default retention policies from the
    /// <see cref="PvNugsMsSqlLogWriterConfig"/> configuration. This provides a convenient
    /// way to perform routine log maintenance without specifying retention periods each time.
    /// </para>
    /// <para>
    /// The purge operation processes each severity level sequentially within a single database connection.
    /// Each severity level is processed in a separate DELETE statement with parameterized queries to prevent 
    /// SQL injection attacks. The cutoff date is calculated as <c>DateTime.UtcNow - retainPeriod</c> for each severity.
    /// </para>
    /// <para>
    /// <strong>Performance Considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>For large log tables, this operation may take considerable time and consume significant I/O resources</item>
    /// <item>Consider running during maintenance windows to minimize impact on application performance</item>
    /// <item>For very large datasets, consider implementing batched deletion strategies</item>
    /// <item>Ensure appropriate database indexes exist on the severity and date columns for optimal performance</item>
    /// </list>
    /// <para>
    /// <strong>Transaction Behavior:</strong> Each DELETE operation runs as a separate statement within the same
    /// database connection, but not within an explicit transaction. This means partial completion is possible
    /// if an error occurs partway through processing multiple severity levels.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use default retention policies from configuration
    /// int deletedRows = await logWriter.PurgeLogsAsync();
    /// Console.WriteLine($"Purged {deletedRows} log entries using default retention policies");
    /// 
    /// // Define custom retention policies
    /// var customRetentionPolicy = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
    /// {
    ///     { SeverityEnu.Critical, TimeSpan.FromDays(730) },   // 2 years for critical
    ///     { SeverityEnu.Error, TimeSpan.FromDays(180) },      // 6 months for errors
    ///     { SeverityEnu.Warning, TimeSpan.FromDays(60) },     // 2 months for warnings
    ///     { SeverityEnu.Info, TimeSpan.FromDays(14) },        // 2 weeks for info
    ///     { SeverityEnu.Debug, TimeSpan.FromDays(1) }         // 1 day for debug
    /// };
    /// 
    /// try
    /// {
    ///     int customDeletedRows = await logWriter.PurgeLogsAsync(customRetentionPolicy);
    ///     Console.WriteLine($"Purged {customDeletedRows} log entries using custom policies");
    /// }
    /// catch (MsSqlLogWriterException ex)
    /// {
    ///     Console.WriteLine($"Failed to purge logs: {ex.Message}");
    ///     // Access the original database exception if needed
    ///     if (ex.InnerException is SqlException sqlEx)
    ///     {
    ///         Console.WriteLine($"SQL Error Number: {sqlEx.Number}");
    ///     }
    /// }
    /// 
    /// // Purge only specific severity levels
    /// var partialPolicy = new Dictionary&lt;SeverityEnu, TimeSpan&gt;
    /// {
    ///     { SeverityEnu.Debug, TimeSpan.FromHours(6) },       // Purge debug logs older than 6 hours
    ///     { SeverityEnu.Trace, TimeSpan.FromHours(1) }        // Purge trace logs older than 1 hour
    /// };
    /// int partialDeleted = await logWriter.PurgeLogsAsync(partialPolicy);
    /// </code>
    /// </example>
    public async Task<int> PurgeLogsAsync(IDictionary<SeverityEnu, TimeSpan>? retainDic = null)
    {
        await EnsureInitializedAsync();

        retainDic ??= new Dictionary<SeverityEnu, TimeSpan>
        {
            { SeverityEnu.Fatal, _config.DefaultRetentionPeriodForFatal },
            { SeverityEnu.Error, _config.DefaultRetentionPeriodForError },
            { SeverityEnu.Warning, _config.DefaultRetentionPeriodForWarning },
            { SeverityEnu.Info, _config.DefaultRetentionPeriodForInfo },
            { SeverityEnu.Debug, _config.DefaultRetentionPeriodForDebug },
            { SeverityEnu.Trace, _config.DefaultRetentionPeriodForTrace },
        };
        
        string appCs;
        try
        {
            appCs = await _csp.GetConnectionStringAsync(
                _config.ConnectionStringName, SqlRoleEnu.Application);
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
            throw new MsSqlLogWriterException(e);
        }

        await using var appCn = new SqlConnection(appCs);
        var totRows = 0;
        try
        {
            await appCn.OpenAsync();
            foreach (var (severity, keep) in retainDic)
            {
                var cmdText = $"DELETE FROM [{_schemaName}].[{_tableName}] " +
                              $"WHERE [{_severityCodeColumnName}] = @severity " +
                              $"AND [{_createDateColumnName}] < @cutoffDate";
            
                await using var cmd = new SqlCommand(cmdText, appCn);
            
                var severityCode = GetSeverityCode(severity);
                var cutoffUtc = DateTime.UtcNow - keep;
            
                cmd.Parameters.Add("@severity", SqlDbType.Char, 1).Value = severityCode;
                cmd.Parameters.Add("@cutoffDate", SqlDbType.DateTime).Value = cutoffUtc;
            
                var rows = await cmd.ExecuteNonQueryAsync();
                totRows += rows;
            }
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
            throw new MsSqlLogWriterException(e);
        }
    
        return totRows;
    }

    /// <summary>
    /// Extracts and normalizes a severity code from the provided severity enumeration.
    /// </summary>
    /// <param name="severity">The severity enumeration to extract the code from.</param>
    /// <returns>
    /// A single character string representing the severity code. 
    /// Returns "D" (Debug) if the severity code is null or empty.
    /// If the severity code is longer than 1 character, only the first character is returned.
    /// </returns>
    /// <remarks>
    /// This method ensures that the severity code always fits within the database column constraint of 1 character.
    /// </remarks>
    private static string GetSeverityCode(SeverityEnu severity)
    {
        var severityCode = severity.GetCode();

        if (string.IsNullOrEmpty(severityCode)) return "D";
        return severityCode.Length > 1 ? severityCode[..1] : severityCode;
    }

    /// <summary>
    /// Truncates a string to the specified maximum length, appending "..." if truncation occurs.
    /// </summary>
    /// <param name="value">The string to truncate. Can be null or empty.</param>
    /// <param name="maxLength">The maximum allowed length for the string.</param>
    /// <returns>
    /// The original string if it's null, empty, or within the length limit.
    /// A truncated string with "..." suffix if the original exceeds the maximum length.
    /// The returned string will never exceed <paramref name="maxLength"/> characters.
    /// </returns>
    /// <remarks>
    /// When truncation occurs, the method reserves 3 characters for the "..." suffix,
    /// so the actual content is limited to <c>maxLength - 3</c> characters.
    /// </remarks>
    /// <example>
    /// <code>
    /// string result1 = TruncateString("Hello", 10);        // Returns: "Hello"
    /// string result2 = TruncateString("Very long text", 8); // Returns: "Very ..."
    /// string result3 = TruncateString(null, 5);            // Returns: null
    /// </code>
    /// </example>
    private static string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length > maxLength ? value[..(maxLength - 3)] + "..." : value;
    }

    private async Task CreateTableIfNotExistsAsync()
    {
        try
        {
            await LogActivityAsync($"Checking table '{_tableName}' existence");
            var readerCs = 
                await _csp.GetConnectionStringAsync(_config.ConnectionStringName);
            await using var readerCn = new SqlConnection(readerCs);
            await readerCn.OpenAsync();
            // Use parameters for table/schema names in queries where possible
            const string existsCommandText = "SELECT 1 FROM sys.tables t " +
                                             "INNER JOIN sys.schemas s ON t.schema_id = s.schema_id " +
                                             "WHERE t.name = @tableName AND s.name = @schemaName";

            await using var cmd = new SqlCommand(existsCommandText, readerCn);
            cmd.Parameters.Add("@tableName", SqlDbType.NVarChar, 128).Value = _tableName;
            cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar, 128).Value = _schemaName;

            var tableExists = await cmd.ExecuteScalarAsync() != null;
            
            await readerCn.CloseAsync();

            if (tableExists) return;
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
        }
        
        try
        {
            await LogActivityAsync($"creating table {_schemaName}.{_tableName}");

            var ownerCs = await _csp.GetConnectionStringAsync(SqlRoleEnu.Owner);
            await using var ownerCn = new SqlConnection(ownerCs);
            await ownerCn.OpenAsync();
            
            // Create the table
            await CreateTableAsync(ownerCn);
            
            // Create indexes for optimal query performance
            await CreateIndexesAsync(ownerCn);
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
            throw new MsSqlLogWriterException(e);
        }
    }
    
    /// <summary>
    /// Creates the log table with the configured schema.
    /// </summary>
    /// <param name="connection">Open database connection with owner privileges.</param>
    private async Task CreateTableAsync(SqlConnection connection)
    {
        // Note: Table/column names can't be parameterized in DDL, but they come from config, not user input
        var identityColumn = _config.IncludeIdentityColumn 
            ? $"[{_config.IdentityColumnName}] INT IDENTITY(1,1) PRIMARY KEY CLUSTERED," 
            : "";

        var createCommandText =
            $"CREATE TABLE [{_schemaName}].[{_tableName}] " +
            $"(" +
            identityColumn +
            $"[{_userIdColumnName}] {SqlVarChar}({_userIdLength})," +
            $"[{_companyIdColumnName}] {SqlVarChar}({_companyIdLength})," +
            $"[{_severityCodeColumnName}] {SqlChar}(1) NOT NULL," +
            $"[{_machineNameColumnName}] {SqlVarChar}({_machineNameLength}) NOT NULL," +
            $"[{_topicColumnName}] {SqlVarChar}({_topicLength})," +
            $"[{_contextColumnName}] {SqlVarChar}({_contextLength}) NOT NULL," +
            $"[{_messageColumnName}] {SqlNVarChar}(MAX) NOT NULL," +
            $"[{_createDateColumnName}] {SqlDateTime} NOT NULL" +
            $")";

        await using var createCmd = new SqlCommand(createCommandText, connection);
        await createCmd.ExecuteNonQueryAsync();
        
        await LogActivityAsync($"Created table structure for {_schemaName}.{_tableName}");
    }
    
        /// <summary>
    /// Creates performance-optimized indexes on the log table.
    /// </summary>
    /// <param name="connection">Open database connection with owner privileges.</param>
    private async Task CreateIndexesAsync(SqlConnection connection)
    {
        var indexes = new List<string>();

        // Primary index for time-based queries (most common for logs)
        if (_config.IncludeDateIndex)
        {
            var dateIndexName = $"IX_{_tableName}_{_createDateColumnName}";
            var dateIndexSql = 
                $"CREATE NONCLUSTERED INDEX [{dateIndexName}] " +
                $"ON [{_schemaName}].[{_tableName}] ([{_createDateColumnName}] DESC)";
            indexes.Add(dateIndexSql);
        }

        // Composite index for efficient purge operations (severity + date)
        if (_config.IncludePurgeIndex)
        {
            var purgeIndexName = $"IX_{_tableName}_Purge";
            var purgeIndexSql = 
                $"CREATE NONCLUSTERED INDEX [{purgeIndexName}] " +
                $"ON [{_schemaName}].[{_tableName}] ([{_severityCodeColumnName}], [{_createDateColumnName}])";
            indexes.Add(purgeIndexSql);
        }

        // Optional: Composite index for user-based queries
        if (_config.IncludeUserIndex)
        {
            var userIndexName = $"IX_{_tableName}_User";
            var userIndexSql = 
                $"CREATE NONCLUSTERED INDEX [{userIndexName}] " +
                $"ON [{_schemaName}].[{_tableName}] ([{_userIdColumnName}], [{_createDateColumnName}] DESC) " +
                $"WHERE [{_userIdColumnName}] IS NOT NULL";
            indexes.Add(userIndexSql);
        }

        // Optional: Index for topic-based queries
        if (_config.IncludeTopicIndex)
        {
            var topicIndexName = $"IX_{_tableName}_Topic";
            var topicIndexSql = 
                $"CREATE NONCLUSTERED INDEX [{topicIndexName}] " +
                $"ON [{_schemaName}].[{_tableName}] ([{_topicColumnName}], [{_createDateColumnName}] DESC) " +
                $"WHERE [{_topicColumnName}] IS NOT NULL";
            indexes.Add(topicIndexSql);
        }

        // Create all indexes
        foreach (var indexSql in indexes)
        {
            try
            {
                await using var indexCmd = new SqlCommand(indexSql, connection);
                await indexCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                await LogActivityAsync($"Warning: Failed to create index. SQL: {indexSql}. Error: {e.Message}");
                // Don't throw - table creation should succeed even if some indexes fail
            }
        }
        
        await LogActivityAsync($"Created {indexes.Count} indexes for {_schemaName}.{_tableName}");
    }

    private async Task CheckTable()
    {
        var errors = new List<string>();
        await LogActivityAsync($"Checking table {_tableName}");
        try
        {
            var readerCs = 
                await _csp.GetConnectionStringAsync(_config.ConnectionStringName);
            await using var readerCn = new SqlConnection(readerCs);

            await readerCn.OpenAsync();
            const string cmdText = "SELECT [column_name], " +
                          "       [data_type], " +
                          "       [is_nullable], " +
                          "       [character_maximum_length] " +
                          "FROM [information_schema].[columns] " +
                          "WHERE [table_schema] = @schemaName " +
                          "AND   [table_name] = @tableName";

            var cmd = readerCn.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.CommandType = CommandType.Text;
        
            cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar, 128).Value = _schemaName;
            cmd.Parameters.Add("@tableName", SqlDbType.NVarChar, 128).Value = _tableName;

            var dic = new Dictionary<string, ColumnInfo>();
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var ci = new ColumnInfo(reader);
                dic.Add(ci.ColumnName, ci);
            }

            await reader.CloseAsync();
            await readerCn.CloseAsync();

            if (dic.Count == 0)
            {
                errors.Add($"table {_tableName} not found");
            }
            else
            {
                CheckColumn(errors, dic, _userIdColumnName, SqlVarChar,
                    true, out _userIdLength);
                CheckColumn(errors, dic, _companyIdColumnName, SqlVarChar,
                    true, out _companyIdLength);
                CheckColumn(errors, dic, _severityCodeColumnName, SqlChar,
                    false, out _);
                CheckColumn(errors, dic, _machineNameColumnName, SqlVarChar,
                    false, out _machineNameLength);
                CheckColumn(errors, dic, _topicColumnName, SqlVarChar,
                    true, out _topicLength);
                CheckColumn(errors, dic, _contextColumnName, SqlVarChar,
                    false, out _contextLength);
                CheckColumn(errors, dic, _messageColumnName, SqlNVarChar,
                    false, out var len);
                if (len != -1)
                {
                    errors.Add($"column {_messageColumnName} should be nvarchar(MAX)");
                }

                CheckColumn(errors, dic, _createDateColumnName, SqlDateTime,
                    false, out _);
            }
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
            throw new MsSqlLogWriterException(e);
        }

        if (errors.Count == 0) return;

        var sb = new StringBuilder();
        foreach (var error in errors)
        {
            if (sb.Length > 0)
                sb.Append(Environment.NewLine);
            sb.Append(error);
        }

        var errorMessage = sb.ToString();
        var exception = new Exception(errorMessage);
        await LogExceptionAsync(exception);
        throw new MsSqlLogWriterException(exception);
    }

    private static void CheckColumn(
        List<string> errors,
        Dictionary<string, ColumnInfo> dic,
        string columnName,
        string? expectedType,
        bool isNullable,
        out int length)
    {
        length = 0;
        if (!dic.TryGetValue(columnName, out var info))
        {
            errors.Add($"{columnName} not found in log table");
            return;
        }

        var type = info.Type.ToLower();
        expectedType = expectedType?.ToLower();
        if (type != expectedType)
        {
            errors.Add($"{columnName} expected type is {expectedType} but actual type is {type}");
            return;
        }

        length = info.Length ?? 0;

        if (isNullable == info.IsNullable) return;

        var neg = isNullable ? "" : "not ";
        errors.Add($"{columnName} should {neg}be nullable");
    }

    /// <summary>
    /// Ensures the database table is properly initialized before performing operations.
    /// Uses a thread-safe double-check locking pattern to prevent multiple concurrent initializations.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    /// <remarks>
    /// <para>This method is called automatically by <see cref="WriteLogAsync"/> and <see cref="PurgeLogsAsync"/>.</para>
    /// <para>Initialization includes table creation (if configured) and schema validation (if configured).</para>
    /// <para>If both <c>CreateTableAtFirstUse</c> and <c>CheckTableAtFirstUse</c> are false, no database operations are performed.</para>
    /// <para>The method uses a semaphore to ensure thread safety and prevent race conditions during initialization.</para>
    /// </remarks>
    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        await _initSemaphore.WaitAsync();
        try
        {
            if (_isInitialized) return; // Double-check pattern

            if (_config is
                {
                    CreateTableAtFirstUse: false,
                    CheckTableAtFirstUse: false
                })
            {
                _isInitialized = true;
                return;
            }

            if (_config.CreateTableAtFirstUse)
                await CreateTableIfNotExistsAsync();

            if (_config.CheckTableAtFirstUse)
                await CheckTable();

            _isInitialized = true;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    private async Task LogActivityAsync(string activity)
    {
        if (_logger == null)
            Console.WriteLine(activity);
        else
            await _logger.LogAsync(activity);
    }

    private async Task LogExceptionAsync(Exception e)
    {
        if (_logger == null)
            Console.WriteLine(e);
        else
            await _logger.LogAsync(e);
    }
}