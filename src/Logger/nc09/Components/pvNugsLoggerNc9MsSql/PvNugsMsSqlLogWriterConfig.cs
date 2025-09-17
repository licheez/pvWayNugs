namespace pvNugsLoggerNc9MsSql;

/// <summary>
/// Provides configuration settings for the Microsoft SQL Server log writer implementation.
/// </summary>
/// <remarks>
/// <para>
/// This configuration class defines all customizable aspects of the SQL Server logging implementation,
/// including table structure, column names, column lengths, and initialization behavior. It is designed to work with
/// the .NET configuration system and can be populated from appsettings.json, environment variables,
/// or other configuration sources.
/// </para>
/// <para>
/// The configuration supports flexible table and column naming, as well as customizable column lengths
/// to accommodate existing database schemas and naming conventions. All string properties have sensible
/// defaults that follow common database naming practices.
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
///     "ContextColumnLength": 2048
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
}