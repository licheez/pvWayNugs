using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10PgSql;

/// <summary>
/// Configuration settings for the PostgreSQL log writer.
/// </summary>
public class PvNugsPgSqlLogWriterConfig
{
    /// <summary>
    /// The configuration section name for the PostgreSQL log writer settings.
    /// </summary>
    public const string Section = nameof(PvNugsPgSqlLogWriterConfig);
    
    /// <summary>
    /// Gets or sets the named connection configuration used by the PostgreSQL cs-provider.
    /// </summary>
    /// <remarks>
    /// This value is passed to <c>IPvNugsPgSqlCsProvider.GetConnectionStringAsync</c> to resolve
    /// role-based connection strings for reader, application, and owner operations.
    /// </remarks>
    public string ConnectionStringName { get; set; } = "Default";

    /// <summary>
    /// Gets or sets the name of the table where logs are stored.
    /// </summary>
    public string TableName { get; set; } = "Log";

    /// <summary>
    /// Gets or sets the name of the column that stores the user ID.
    /// </summary>
    public string UserIdColumnName { get; set; } = "UserId";

    /// <summary>
    /// Gets the name of the column that stores the company ID.
    /// </summary>
    public string CompanyIdColumnName { get; set; } = "CompanyId";

    /// <summary>
    /// Gets or sets the name of the column that stores the machine name.
    /// </summary>
    public string MachineNameColumnName { get; set; } = "MachineName";

    /// <summary>
    /// Gets or sets the name of the column that stores the severity code.
    /// </summary>
    public string SeverityCodeColumnName { get; set; } = "SeverityCode";

    /// <summary>
    /// Gets or sets the name of the column that stores the context.
    /// </summary>
    public string ContextColumnName { get; set; } = "Context";

    /// <summary>
    /// Gets or sets the name of the column that stores the topic.
    /// </summary>
    public string TopicColumnName { get; set; } = "Topic";

    /// <summary>
    /// Gets or sets the name of the column that stores the message.
    /// </summary>
    public string MessageColumnName { get; set; } = "Message";

    /// <summary>
    /// Gets or sets the name of the column that stores the creation date in UTC.
    /// </summary>
    public string CreateDateUtcColumnName { get; set; } = "CreateDateUtc";

    /// <summary>
    /// Gets or sets a value indicating whether to create the log table on application start.
    /// </summary>
    public bool CreateTableAtFirstUse { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to check the log table on application start.
    /// </summary>
    public bool CheckTableAtFirstUse { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the default retention period for Fatal severity logs when purging operations are performed.
    /// </summary>
    /// <value>
    /// The retention period for Fatal logs. Default value is 365 days (1 year).
    /// </value>
    /// <remarks>
    /// <para>
    /// Fatal logs typically contain critical system failures, security incidents, and catastrophic errors
    /// that require extended retention for compliance, forensic analysis, and regulatory purposes.
    /// The extended retention period reflects the critical nature of these events.
    /// </para>
    /// <para>
    /// <strong>Usage in Purge Operations:</strong>
    /// </para>
    /// <para>
    /// This value is used when <see cref="PgSqlLogWriter.PurgeLogsAsync(IDictionary{SeverityEnu, TimeSpan}?, CancellationToken)"/>
    /// is called with a null retention dictionary parameter, implementing the three-tier decision cascade:
    /// </para>
    /// <list type="number">
    /// <item><strong>Option 1:</strong> If a custom retention dictionary is passed to PurgeLogsAsync, those values are used instead</item>
    /// <item><strong>Option 2:</strong> If no custom dictionary is provided AND this property is not configured in settings, this default value (365 days) is used</item>
    /// <item><strong>Option 3:</strong> If no custom dictionary is provided AND this property IS configured in settings (e.g., appsettings.json), the configured value is used</item>
    /// </list>
    /// <para>
    /// <strong>Configuration Examples:</strong>
    /// </para>
    /// <para>
    /// You can override this default through configuration:
    /// </para>
    /// <code>
    /// // appsettings.json
    /// {
    ///   "PvNugsPgSqlLogWriterConfig": {
    ///     "DefaultRetentionPeriodForFatal": "730.00:00:00"  // 2 years for compliance
    ///   }
    /// }
    /// 
    /// // Or programmatically
    /// builder.Services.Configure&lt;PvNugsPgSqlLogWriterConfig&gt;(options =&gt;
    /// {
    ///     options.DefaultRetentionPeriodForFatal = TimeSpan.FromDays(1095); // 3 years
    /// });
    /// </code>
    /// <para>
    /// <strong>Compliance Considerations:</strong>
    /// </para>
    /// <para>
    /// Consider your organization's compliance requirements when setting this value:
    /// </para>
    /// <list type="bullet">
    /// <item>Financial services: Often require 7+ years retention for critical system events</item>
    /// <item>Healthcare: May require extended retention for security and audit events</item>
    /// <item>General enterprise: 1-2 years is typically sufficient for operational needs</item>
    /// <item>Development environments: Can be set to shorter periods (days or weeks) for cost management</item>
    /// </list>
    /// </remarks>
    public TimeSpan DefaultRetentionPeriodForFatal { get; set; } = TimeSpan.FromDays(365);

    /// <summary>
    /// Gets or sets the default retention period for Error severity logs when purging operations are performed.
    /// </summary>
    /// <value>
    /// The retention period for Error logs. Default value is 90 days (3 months).
    /// </value>
    /// <remarks>
    /// <para>
    /// Error logs contain application errors, exceptions, and system failures that require sufficient
    /// retention for troubleshooting, root cause analysis, and pattern identification. The 90-day
    /// default provides adequate time for investigation while managing storage costs.
    /// </para>
    /// <para>
    /// This value follows the same three-tier decision cascade as other retention periods.
    /// See <see cref="DefaultRetentionPeriodForFatal"/> for detailed cascade behavior documentation.
    /// </para>
    /// <para>
    /// <strong>Operational Considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>90 days allows for monthly analysis cycles and quarterly reviews</item>
    /// <item>Sufficient time for delayed bug reports and customer escalations</item>
    /// <item>Enables trend analysis for recurring error patterns</item>
    /// <item>Balances investigative needs with storage cost management</item>
    /// </list>
    /// </remarks>
    public TimeSpan DefaultRetentionPeriodForError { get; set; } = TimeSpan.FromDays(90);

    /// <summary>
    /// Gets or sets the default retention period for Warning severity logs when purging operations are performed.
    /// </summary>
    /// <value>
    /// The retention period for Warning logs. Default value is 30 days (1 month).
    /// </value>
    /// <remarks>
    /// <para>
    /// Warning logs contain potential issues, performance degradations, and anomalies that are useful
    /// for monitoring trends and identifying patterns over a moderate time period. The 30-day retention
    /// provides sufficient data for monthly operational reviews.
    /// </para>
    /// <para>
    /// This value follows the same three-tier decision cascade as other retention periods.
    /// See <see cref="DefaultRetentionPeriodForFatal"/> for detailed cascade behavior documentation.
    /// </para>
    /// </remarks>
    public TimeSpan DefaultRetentionPeriodForWarning { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Gets or sets the default retention period for Info severity logs when purging operations are performed.
    /// </summary>
    /// <value>
    /// The retention period for Info logs. Default value is 7 days (1 week).
    /// </value>
    /// <remarks>
    /// <para>
    /// Info logs contain general operational information, successful operations, and routine system
    /// events that are useful for short-term monitoring and recent activity analysis. The 7-day
    /// retention covers typical operational review cycles while keeping storage requirements manageable.
    /// </para>
    /// <para>
    /// This value follows the same three-tier decision cascade as other retention periods.
    /// See <see cref="DefaultRetentionPeriodForFatal"/> for detailed cascade behavior documentation.
    /// </para>
    /// <para>
    /// <strong>Volume Considerations:</strong>
    /// </para>
    /// <para>
    /// Info logs typically represent the highest volume of log entries in most applications.
    /// Consider your storage capacity and query performance when adjusting this value:
    /// </para>
    /// <list type="bullet">
    /// <item>High-traffic applications may need shorter retention (1-3 days)</item>
    /// <item>Low-traffic applications can afford longer retention (14-30 days)</item>
    /// <item>Development environments may use very short retention (hours or 1 day)</item>
    /// </list>
    /// </remarks>
    public TimeSpan DefaultRetentionPeriodForInfo { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets the default retention period for Debug severity logs when purging operations are performed.
    /// </summary>
    /// <value>
    /// The retention period for Debug logs. Default value is 1 day.
    /// </value>
    /// <remarks>
    /// <para>
    /// Debug logs contain detailed diagnostic information, method entry/exit traces, and verbose
    /// operational details that are typically only needed for immediate troubleshooting and
    /// development activities. The short retention period reflects their high volume and temporary utility.
    /// </para>
    /// <para>
    /// This value follows the same three-tier decision cascade as other retention periods.
    /// See <see cref="DefaultRetentionPeriodForFatal"/> for detailed cascade behavior documentation.
    /// </para>
    /// <para>
    /// <strong>Performance Impact:</strong>
    /// </para>
    /// <para>
    /// Debug logs can significantly impact both storage and query performance due to their volume:
    /// </para>
    /// <list type="bullet">
    /// <item>Production environments should minimize debug logging or use very short retention</item>
    /// <item>Development/staging environments can use longer retention for active debugging</item>
    /// <item>Consider disabling debug logging entirely in high-performance production scenarios</item>
    /// </list>
    /// </remarks>
    public TimeSpan DefaultRetentionPeriodForDebug { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// Gets or sets the default retention period for Trace severity logs when purging operations are performed.
    /// </summary>
    /// <value>
    /// The retention period for Trace logs. Default value is 1 hour.
    /// </value>
    /// <remarks>
    /// <para>
    /// Trace logs contain the most verbose diagnostic information including detailed execution paths,
    /// variable states, and fine-grained operational details. These logs are typically only needed
    /// for immediate debugging sessions and active troubleshooting. The very short retention period
    /// helps manage storage costs for extremely high-volume trace logging.
    /// </para>
    /// <para>
    /// This value follows the same three-tier decision cascade as other retention periods.
    /// See <see cref="DefaultRetentionPeriodForFatal"/> for detailed cascade behavior documentation.
    /// </para>
    /// <para>
    /// <strong>Usage Patterns:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><strong>Production:</strong> Trace logging should be disabled or limited to critical components with very short retention</item>
    /// <item><strong>Staging/Testing:</strong> Can use longer retention (hours to days) for integration testing scenarios</item>
    /// <item><strong>Development:</strong> May use extended retention for active debugging sessions</item>
    /// <item><strong>Troubleshooting:</strong> Enable temporarily with immediate analysis, then disable</item>
    /// </list>
    /// <para>
    /// <strong>Storage and Performance Considerations:</strong>
    /// </para>
    /// <para>
    /// Trace logs can generate enormous volumes of data and severely impact system performance:
    /// </para>
    /// <list type="bullet">
    /// <item>Can generate thousands of log entries per second in busy applications</item>
    /// <item>May require frequent purging (multiple times per day) to manage storage</item>
    /// <item>Consider using separate trace-specific retention policies for different components</item>
    /// <item>Monitor database size and query performance when using trace logging</item>
    /// </list>
    /// <para>
    /// <strong>Configuration Examples for Different Scenarios:</strong>
    /// </para>
    /// <code>
    /// // Development environment - longer retention for active debugging
    /// {
    ///   "PvNugsPgSqlLogWriterConfig": {
    ///     "DefaultRetentionPeriodForTrace": "24:00:00"  // 24 hours
    ///   }
    /// }
    /// 
    /// // Production environment - minimal retention
    /// {
    ///   "PvNugsPgSqlLogWriterConfig": {
    ///     "DefaultRetentionPeriodForTrace": "00:10:00"  // 10 minutes
    ///   }
    /// }
    /// 
    /// // Testing/CI environment - very short retention
    /// {
    ///   "PvNugsPgSqlLogWriterConfig": {
    ///     "DefaultRetentionPeriodForTrace": "00:00:30"  // 30 seconds
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public TimeSpan DefaultRetentionPeriodForTrace { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets a value indicating whether to create a descending date index on the UTC date column.
    /// </summary>
    public bool IncludeDateIndex { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to create a composite index optimized for purge operations.
    /// </summary>
    public bool IncludePurgeIndex { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to create a partial index for user-focused queries.
    /// </summary>
    public bool IncludeUserIndex { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to create a partial index for topic-focused queries.
    /// </summary>
    public bool IncludeTopicIndex { get; set; } = true;
}