using pvNugsLoggerNc6Abstractions;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace pvNugsLoggerNc6MsSql;

/// <summary>
/// Provides configuration settings for the Microsoft SQL Server log writer implementation.
/// </summary>
/// <remarks>
/// <para>
/// This configuration class defines all customizable aspects of the SQL Server logging implementation,
/// including table structure, column names, column lengths, initialization behavior, and retention policies. 
/// It is designed to work with the .NET configuration system and can be populated from appsettings.json, 
/// environment variables, or other configuration sources.
/// </para>
/// <para>
/// The configuration supports flexible table and column naming, customizable column lengths, and 
/// configurable log retention policies to accommodate existing database schemas and operational requirements. 
/// All properties have sensible defaults that follow common database naming practices and enterprise 
/// logging standards.
/// </para>
/// <para>
/// <strong>Important:</strong> Changes to table structure configuration (table name, schema, column names, lengths)
/// should be made before first use of the log writer, as the table validation occurs on first log operation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // appsettings.json configuration example
/// {
///   "PvNugsMsSqlLogWriterConfig": {
///     "TableName": "ApplicationLogs",
///     "SchemaName": "Logging",
///     "CreateTableAtFirstUse": true,
///     "CheckTableAtFirstUse": true,
///     "UserIdColumnName": "UserId",
///     "UserIdColumnLength": 256,
///     "CompanyIdColumnName": "CompanyId",
///     "CompanyIdColumnLength": 64,
///     "SeverityCodeColumnName": "LogLevel",
///     "MessageColumnName": "LogMessage",
///     "ContextColumnLength": 2048,
///     "DefaultRetentionPeriodForFatal": "730.00:00:00",
///     "DefaultRetentionPeriodForError": "180.00:00:00",
///     "DefaultRetentionPeriodForWarning": "60.00:00:00",
///     "DefaultRetentionPeriodForInfo": "14.00:00:00",
///     "DefaultRetentionPeriodForDebug": "1.00:00:00",
///     "DefaultRetentionPeriodForTrace": "01:00:00"
///   }
/// }
/// 
/// // Programmatic configuration in Program.cs or Startup.cs
/// builder.Services.Configure&lt;PvNugsMsSqlLogWriterConfig&gt;(options =&gt;
/// {
///     options.TableName = "CustomLogTable";
///     options.SchemaName = "audit";
///     options.UserIdColumnLength = 100;
///     options.TopicColumnLength = 200;
///     options.CreateTableAtFirstUse = false; // Use existing table
///     
///     // Configure custom retention policies
///     options.DefaultRetentionPeriodForFatal = TimeSpan.FromDays(1095); // 3 years
///     options.DefaultRetentionPeriodForError = TimeSpan.FromDays(365);  // 1 year
/// });
/// 
/// // Registration with DI container
/// builder.Services.AddSingleton&lt;IMsSqlLogWriter, MsSqlLogWriter&gt;();
/// builder.Services.AddSingleton&lt;IMsSqlLoggerService, MsSqlLoggerService&gt;();
/// </code>
/// </example>
public class PvNugsMsSqlLogWriterConfig
{
    /// <summary>
    /// Gets the configuration section name used for binding from configuration sources.
    /// </summary>
    /// <value>
    /// The string "PvNugsMsSqlLogWriterConfig" which corresponds to the configuration section name
    /// in appsettings.json or other configuration sources.
    /// </value>
    /// <remarks>
    /// Use this constant when configuring the options in dependency injection setup or when
    /// manually binding configuration sections.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using the Section constant for configuration binding
    /// builder.Services.Configure&lt;PvNugsMsSqlLogWriterConfig&gt;(
    ///     builder.Configuration.GetSection(PvNugsMsSqlLogWriterConfig.Section));
    /// </code>
    /// </example>
    public const string Section = nameof(PvNugsMsSqlLogWriterConfig);

    ///<summary>
    /// Gets or sets the name of the connection string configuration to use for the Logging database.
    /// </summary>
    /// <remarks> This property allows specifying which connection string from the configuration should be used
    /// when writing logs, supporting scenarios with multiple database connections.
    /// </remarks>
    public string ConnectionStringName { get; set; } = "Default";
    
    /// <summary>
    /// Gets or sets the name of the database table used for storing log entries.
    /// </summary>
    /// <value>
    /// The table name. Default value is "Log".
    /// </value>
    /// <remarks>
    /// <para>
    /// The table name should follow SQL Server naming conventions and be unique within the specified schema.
    /// If <see cref="CreateTableAtFirstUse"/> is true, this table will be created automatically on first use.
    /// </para>
    /// <para>
    /// <strong>Security Note:</strong> While this value is used in dynamic SQL for DDL operations,
    /// it comes from configuration rather than user input, reducing SQL injection risk. However,
    /// ensure this value is properly validated in your configuration management.
    /// </para>
    /// </remarks>
    public string TableName { get; set; } = "Log";
    
    /// <summary>
    /// Gets or sets the database schema name containing the log table.
    /// </summary>
    /// <value>
    /// The schema name. Default value is "dbo".
    /// </value>
    /// <remarks>
    /// <para>
    /// The schema must exist in the database before the log writer attempts to create or access the table.
    /// Common schema names include "dbo" (default), "logging", "audit", or custom application schemas.
    /// </para>
    /// <para>
    /// Ensure that the database user has appropriate permissions on this schema for the operations
    /// you intend to perform (SELECT, INSERT, DELETE for normal operations, plus CREATE TABLE if
    /// using automatic table creation).
    /// </para>
    /// </remarks>
    public string SchemaName { get; set; } = "dbo";
    
    /// <summary>
    /// Gets or sets a value indicating whether an identity column should be included in the log table.
    /// </summary>
    /// <value>
    /// <c>true</c> if an identity column should be created; otherwise, <c>false</c>. Default value is <c>true</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, an auto-incrementing integer identity column will be created as the primary key
    /// of the log table. This provides unique identification for each log entry and improves query performance.
    /// </para>
    /// <para>
    /// Set to <c>false</c> if your existing table doesn't have an identity column or if you prefer a different
    /// primary key strategy.
    /// </para>
    /// </remarks>
    public bool IncludeIdentityColumn { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the name of the identity column.
    /// </summary>
    /// <value>
    /// The identity column name. Default value is "Id".
    /// </value>
    /// <remarks>
    /// <para>
    /// This setting is only used when <see cref="IncludeIdentityColumn"/> is <c>true</c>.
    /// The identity column will be created as INT IDENTITY(1,1) PRIMARY KEY.
    /// </para>
    /// <para>
    /// Common alternative names include "LogId", "EntryId", or "SequenceId" depending on your
    /// naming conventions.
    /// </para>
    /// </remarks>
    public string IdentityColumnName { get; set; } = "Id";
    
    /// <summary>
    /// Gets or sets a value indicating whether an index should be created on the CreateDateUtc column.
    /// </summary>
    /// <value>
    /// <c>true</c> if a date index should be created; otherwise, <c>false</c>. Default value is <c>true</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, a non-clustered index will be created on the CreateDateUtc column in descending order,
    /// optimizing time-based queries which are very common in logging scenarios.
    /// </para>
    /// <para>
    /// This index is highly recommended for log tables as most log queries involve date range filtering.
    /// </para>
    /// </remarks>
    public bool IncludeDateIndex { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a composite index should be created for purge operations.
    /// </summary>
    /// <value>
    /// <c>true</c> if a purge index should be created; otherwise, <c>false</c>. Default value is <c>true</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, a composite non-clustered index will be created on (SeverityCode, CreateDateUtc)
    /// to optimize the performance of log purging operations which filter by both severity and date.
    /// </para>
    /// <para>
    /// This index significantly improves the performance of the PurgeLogsAsync method.
    /// </para>
    /// </remarks>
    public bool IncludePurgeIndex { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether an index should be created for user-based queries.
    /// </summary>
    /// <value>
    /// <c>true</c> if a user index should be created; otherwise, <c>false</c>. Default value is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, a filtered non-clustered index will be created on (UserId, CreateDateUtc)
    /// where UserId is not null, optimizing queries that filter logs by specific users.
    /// </para>
    /// <para>
    /// Enable this only if you frequently query logs by user ID, as it adds storage overhead.
    /// </para>
    /// </remarks>
    public bool IncludeUserIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an index should be created for topic-based queries.
    /// </summary>
    /// <value>
    /// <c>true</c> if a topic index should be created; otherwise, <c>false</c>. Default value is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, a filtered non-clustered index will be created on (Topic, CreateDateUtc)
    /// where Topic is not null, optimizing queries that filter logs by specific topics or categories.
    /// </para>
    /// <para>
    /// Enable this only if you frequently query logs by topic, as it adds storage overhead.
    /// </para>
    /// </remarks>
    public bool IncludeTopicIndex { get; set; }

    /// <summary>
    /// Gets or sets the name of the column that stores user identifiers.
    /// </summary>
    /// <value>
    /// The user ID column name. Default value is "UserId".
    /// </value>
    /// <remarks>
    /// <para>
    /// This column stores the user context for log entries and can be null. The column will be created
    /// as VARCHAR with the length specified by <see cref="UserIdColumnLength"/>.
    /// </para>
    /// <para>
    /// Common alternative names include "UserName", "User_Id", or "LoginId" depending on your
    /// application's user identification scheme.
    /// </para>
    /// </remarks>
    public string UserIdColumnName { get; set; } = "UserId";
    
    /// <summary>
    /// Gets or sets the maximum length for the user ID column.
    /// </summary>
    /// <value>
    /// The column length in characters. Default value is 128.
    /// </value>
    /// <remarks>
    /// <para>
    /// This value determines the VARCHAR length for the user ID column when the table is created automatically.
    /// User IDs longer than this value will be automatically truncated with "..." suffix to prevent database errors.
    /// </para>
    /// <para>
    /// Consider your application's user identification scheme when setting this value. Common lengths:
    /// </para>
    /// <list type="bullet">
    /// <item>50-100 characters for typical username schemes</item>
    /// <item>128-256 characters for email addresses or longer identifiers</item>
    /// <item>36-50 characters for GUID-based user IDs</item>
    /// </list>
    /// </remarks>
    public int UserIdColumnLength { get; set; } = 128;
    
    /// <summary>
    /// Gets or sets the name of the column that stores company or organization identifiers.
    /// </summary>
    /// <value>
    /// The company ID column name. Default value is "CompanyId".
    /// </value>
    /// <remarks>
    /// <para>
    /// This column stores the company/organization context for log entries in multi-tenant applications
    /// and can be null. The column will be created as VARCHAR with the length specified by <see cref="CompanyIdColumnLength"/>.
    /// </para>
    /// <para>
    /// Alternative names might include "OrganizationId", "TenantId", "Company_Id", or "ClientId"
    /// depending on your application's multi-tenancy model.
    /// </para>
    /// </remarks>
    public string CompanyIdColumnName { get; set; } = "CompanyId";
    
    /// <summary>
    /// Gets or sets the maximum length for the company ID column.
    /// </summary>
    /// <value>
    /// The column length in characters. Default value is 128.
    /// </value>
    /// <remarks>
    /// <para>
    /// This value determines the VARCHAR length for the company ID column when the table is created automatically.
    /// Company IDs longer than this value will be automatically truncated with "..." suffix to prevent database errors.
    /// </para>
    /// <para>
    /// Consider your application's multi-tenancy model when setting this value. Common patterns:
    /// </para>
    /// <list type="bullet">
    /// <item>50-100 characters for company codes or short names</item>
    /// <item>128-256 characters for full company names or longer identifiers</item>
    /// <item>36-50 characters for GUID-based tenant IDs</item>
    /// </list>
    /// </remarks>
    public int CompanyIdColumnLength { get; set; } = 128;
    
    /// <summary>
    /// Gets or sets the name of the column that stores the machine name where the log entry originated.
    /// </summary>
    /// <value>
    /// The machine name column name. Default value is "MachineName".
    /// </value>
    /// <remarks>
    /// <para>
    /// This column stores the name of the server or machine where the log entry was generated,
    /// which is useful for distributed applications and troubleshooting. The column will be created
    /// as VARCHAR with the length specified by <see cref="MachineNameColumnLength"/> and cannot be null.
    /// </para>
    /// <para>
    /// The value is automatically populated with <see cref="System.Environment.MachineName"/> if
    /// not provided explicitly in the log call.
    /// </para>
    /// </remarks>
    public string MachineNameColumnName { get; set; } = "MachineName";
    
    /// <summary>
    /// Gets or sets the maximum length for the machine name column.
    /// </summary>
    /// <value>
    /// The column length in characters. Default value is 128.
    /// </value>
    /// <remarks>
    /// <para>
    /// This value determines the VARCHAR length for the machine name column when the table is created automatically.
    /// Machine names longer than this value will be automatically truncated with "..." suffix.
    /// </para>
    /// <para>
    /// Windows machine names are typically limited to 15 characters, but container environments,
    /// cloud services, and other platforms may use longer names. The default of 128 characters
    /// should accommodate most scenarios including:
    /// </para>
    /// <list type="bullet">
    /// <item>Standard Windows computer names (up to 15 characters)</item>
    /// <item>Container names and Kubernetes pod names</item>
    /// <item>Cloud service instance names</item>
    /// <item>Virtual machine names</item>
    /// </list>
    /// </remarks>
    public int MachineNameColumnLength { get; set; } = 128;
    
    /// <summary>
    /// Gets or sets the name of the column that stores log severity codes.
    /// </summary>
    /// <value>
    /// The severity code column name. Default value is "SeverityCode".
    /// </value>
    /// <remarks>
    /// <para>
    /// This column stores a single character representing the log severity level (e.g., 'D' for Debug,
    /// 'I' for Info, 'W' for Warning, 'E' for Error). The column will be created as CHAR(1) and cannot be null.
    /// </para>
    /// <para>
    /// Alternative names might include "LogLevel", "Level", "Severity", or "Priority" depending on
    /// your logging terminology preferences.
    /// </para>
    /// </remarks>
    public string SeverityCodeColumnName { get; set; } = "SeverityCode";
    
    /// <summary>
    /// Gets or sets the name of the column that stores contextual information about where the log originated.
    /// </summary>
    /// <value>
    /// The context column name. Default value is "Context".
    /// </value>
    /// <remarks>
    /// <para>
    /// This column stores formatted contextual information including method name, file path, and line number
    /// in the format: "methodName # filePath # lineNumber". The column will be created as VARCHAR 
    /// with the length specified by <see cref="ContextColumnLength"/> and cannot be null.
    /// </para>
    /// <para>
    /// This information is invaluable for debugging and tracing the source of log entries back to specific
    /// code locations. Alternative names might include "Source", "Origin", "Location", or "Caller".
    /// </para>
    /// </remarks>
    public string ContextColumnName { get; set; } = "Context";
    
    /// <summary>
    /// Gets or sets the maximum length for the context column.
    /// </summary>
    /// <value>
    /// The column length in characters. Default value is 1024.
    /// </value>
    /// <remarks>
    /// <para>
    /// This value determines the VARCHAR length for the context column when the table is created automatically.
    /// Context information longer than this value will be automatically truncated with "..." suffix.
    /// </para>
    /// <para>
    /// The context field combines method name, file path, and line number information. Consider file path lengths
    /// in your development environment when setting this value:
    /// </para>
    /// <list type="bullet">
    /// <item>500-1000 characters for typical development setups</item>
    /// <item>1024-2048 characters for complex project structures or long paths</item>
    /// <item>2048+ characters for very deep directory structures</item>
    /// </list>
    /// <para>
    /// <strong>Performance Note:</strong> Larger column lengths may impact query performance and storage requirements.
    /// </para>
    /// </remarks>
    public int ContextColumnLength { get; set; } = 1024;
    
    /// <summary>
    /// Gets or sets the name of the column that stores topic or category information.
    /// </summary>
    /// <value>
    /// The topic column name. Default value is "Topic".
    /// </value>
    /// <remarks>
    /// <para>
    /// This column stores optional topic or category information that can be used to group related log entries.
    /// The column will be created as VARCHAR with the length specified by <see cref="TopicColumnLength"/> and can be null.
    /// </para>
    /// <para>
    /// Topics are useful for organizing logs by functional area, feature, or business process.
    /// Alternative names might include "Category", "Module", "Feature", or "Component".
    /// </para>
    /// </remarks>
    public string TopicColumnName { get; set; } = "Topic";
    
    /// <summary>
    /// Gets or sets the maximum length for the topic column.
    /// </summary>
    /// <value>
    /// The column length in characters. Default value is 128.
    /// </value>
    /// <remarks>
    /// <para>
    /// This value determines the VARCHAR length for the topic column when the table is created automatically.
    /// Topics longer than this value will be automatically truncated with "..." suffix.
    /// </para>
    /// <para>
    /// Consider your application's topic/category naming conventions when setting this value:
    /// </para>
    /// <list type="bullet">
    /// <item>50-100 characters for simple category names</item>
    /// <item>128-256 characters for hierarchical topics or longer descriptions</item>
    /// <item>Less than 50 characters for short, coded categories</item>
    /// </list>
    /// </remarks>
    public int TopicColumnLength { get; set; } = 128;
    
    /// <summary>
    /// Gets or sets the name of the column that stores the actual log message content.
    /// </summary>
    /// <value>
    /// The message column name. Default value is "Message".
    /// </value>
    /// <remarks>
    /// <para>
    /// This column stores the primary log message content and will be created as NVARCHAR(MAX) to
    /// accommodate messages of any reasonable length. This column cannot be null.
    /// </para>
    /// <para>
    /// The NVARCHAR(MAX) data type supports Unicode characters and can store up to 2GB of text data,
    /// making it suitable for detailed error messages, stack traces, and other verbose log content.
    /// </para>
    /// </remarks>
    public string MessageColumnName { get; set; } = "Message";
    
    /// <summary>
    /// Gets or sets the name of the column that stores the UTC timestamp when the log entry was created.
    /// </summary>
    /// <value>
    /// The create date column name. Default value is "CreateDateUtc".
    /// </value>
    /// <remarks>
    /// <para>
    /// This column stores the UTC timestamp of when the log entry was created and will be created as DATETIME.
    /// The column cannot be null and is essential for log chronology and retention policies.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> All timestamps are stored in UTC to ensure consistency across different
    /// time zones and daylight saving time transitions. Alternative names might include "Timestamp",
    /// "LogDate", "CreatedAt", or "EventTime".
    /// </para>
    /// </remarks>
    public string CreateDateUtcColumnName { get; set; } = "CreateDateUtc";

    /// <summary>
    /// Gets or sets a value indicating whether the log table should be created automatically on first use.
    /// </summary>
    /// <value>
    /// <c>true</c> if the table should be created automatically; otherwise, <c>false</c>. Default value is <c>true</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, the log writer will check if the specified table exists on first use and
    /// create it if it doesn't exist. This requires the database user to have CREATE TABLE permissions
    /// and access to an "Owner" role connection string.
    /// </para>
    /// <para>
    /// Set to <c>false</c> if you prefer to create the log table manually or through database migration scripts.
    /// This is recommended for production environments where database schema changes are controlled through
    /// formal deployment processes.
    /// </para>
    /// <para>
    /// <strong>Security Consideration:</strong> Automatic table creation requires elevated database permissions
    /// that may not be desirable in production environments.
    /// </para>
    /// </remarks>
    public bool CreateTableAtFirstUse { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether the log table schema should be validated on first use.
    /// </summary>
    /// <value>
    /// <c>true</c> if the table schema should be validated; otherwise, <c>false</c>. Default value is <c>true</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, the log writer will validate the existing table schema on first use to ensure
    /// it matches the expected structure (column names, data types, lengths, nullability). This helps catch configuration
    /// errors early and ensures reliable logging operations.
    /// </para>
    /// <para>
    /// Set to <c>false</c> only if you are certain the table structure is correct and want to minimize
    /// startup overhead. Schema validation failures will result in a <see cref="MsSqlLogWriterException"/>
    /// being thrown with details about the schema mismatches.
    /// </para>
    /// <para>
    /// <strong>Recommendation:</strong> Keep this enabled in development and testing environments to catch
    /// configuration issues early. Consider disabling in high-performance production scenarios where
    /// you are confident about the table structure.
    /// </para>
    /// </remarks>
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
    /// This value is used when <see cref="MsSqlLogWriter.PurgeLogsAsync(IDictionary{SeverityEnu, TimeSpan}?)"/>
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
    ///   "PvNugsMsSqlLogWriterConfig": {
    ///     "DefaultRetentionPeriodForFatal": "730.00:00:00"  // 2 years for compliance
    ///   }
    /// }
    /// 
    /// // Or programmatically
    /// builder.Services.Configure&lt;PvNugsMsSqlLogWriterConfig&gt;(options =&gt;
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
    ///   "PvNugsMsSqlLogWriterConfig": {
    ///     "DefaultRetentionPeriodForTrace": "24:00:00"  // 24 hours
    ///   }
    /// }
    /// 
    /// // Production environment - minimal retention
    /// {
    ///   "PvNugsMsSqlLogWriterConfig": {
    ///     "DefaultRetentionPeriodForTrace": "00:10:00"  // 10 minutes
    ///   }
    /// }
    /// 
    /// // Testing/CI environment - very short retention
    /// {
    ///   "PvNugsMsSqlLogWriterConfig": {
    ///     "DefaultRetentionPeriodForTrace": "00:00:30"  // 30 seconds
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public TimeSpan DefaultRetentionPeriodForTrace { get; set; } = TimeSpan.FromHours(1);
}