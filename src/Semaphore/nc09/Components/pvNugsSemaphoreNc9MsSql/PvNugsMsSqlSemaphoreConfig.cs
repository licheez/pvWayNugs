namespace pvNugsSemaphoreNc9MsSql;

/// <summary>
/// Configuration options for the SQL Server-based distributed semaphore (mutex) service.
/// Controls the connection string, table and schema names, and table creation behavior.
/// </summary>
public class PvNugsMsSqlSemaphoreConfig
{
    /// <summary>
    /// The configuration section name for binding <see cref="PvNugsMsSqlSemaphoreConfig"/> from appsettings or other configuration sources.
    /// Use this constant when registering or retrieving configuration for the semaphore service.
    /// </summary>
    public const string Section = nameof(PvNugsMsSqlSemaphoreConfig);
    
    /// <summary>
    /// Gets or sets the name of the connection string configuration to use for the semaphore database.
    /// </summary>
    /// <remarks>
    /// This property allows specifying which connection string from the configuration should be used
    /// when managing semaphores, supporting scenarios with multiple database connections.
    /// </remarks>
    public string ConnectionStringName { get; set; } = "Default";

    /// <summary>
    /// Gets or sets the name of the database table used for storing semaphore entries.
    /// </summary>
    /// <value>
    /// The table name. Default value is "Semaphore".
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
    public string TableName { get; set; } = "Semaphore";
    
    /// <summary>
    /// Gets or sets the database schema name containing the semaphore table.
    /// </summary>
    /// <value>
    /// The schema name. Default value is "dbo".
    /// </value>
    /// <remarks>
    /// <para>
    /// The schema must exist in the database before the semaphore service attempts to create or access the table.
    /// Common schema names include "dbo" (default), "semaphore", or custom application schemas.
    /// </para>
    /// <para>
    /// Ensure that the database user has appropriate permissions on this schema for the operations
    /// you intend to perform (SELECT, INSERT, DELETE for normal operations, plus CREATE TABLE if
    /// using automatic table creation).
    /// </para>
    /// </remarks>
    public string SchemaName { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets a value indicating whether the semaphore table should be created automatically on first use.
    /// </summary>
    /// <value>
    /// <c>true</c> if the table should be created automatically; otherwise, <c>false</c>. Default value is <c>true</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, the semaphore service will check if the specified table exists on first use and
    /// create it if it doesn't exist. This requires the database user to have CREATE TABLE permissions
    /// and access to an "Owner" role connection string.
    /// </para>
    /// <para>
    /// Set to <c>false</c> if you prefer to create the semaphore table manually or through database migration scripts.
    /// This is recommended for production environments where database schema changes are controlled through
    /// formal deployment processes.
    /// </para>
    /// <para>
    /// <strong>Security Consideration:</strong> Automatic table creation requires elevated database permissions
    /// that may not be desirable in production environments.
    /// </para>
    /// </remarks>
    public bool CreateTableAtFirstUse { get; set; } = true;
}