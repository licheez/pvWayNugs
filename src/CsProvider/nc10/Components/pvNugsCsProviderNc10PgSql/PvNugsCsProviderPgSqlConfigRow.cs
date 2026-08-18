// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace pvNugsCsProviderNc10PgSql;

/// <summary>
/// Represents a single PostgreSQL connection configuration row for multi-database support.
/// Contains all necessary parameters for database connections and credential management across different operational modes.
/// </summary>
public class PvNugsCsProviderPgSqlConfigRow
{
    /// <summary>
    /// Gets or sets the unique name/identifier for this configuration row.
    /// Used to distinguish between multiple PostgreSQL connection configurations.
    /// </summary>
    public string Name { get; set; } = "Default";

    /// <summary>
    /// Gets or sets the operational mode for credential management.
    /// Determines which other configuration properties are required and how credentials are retrieved.
    /// </summary>
    public CsProviderModeEnu Mode { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL server hostname or IP address.
    /// Used directly in the PostgreSQL connection string as the Server parameter.
    /// </summary>
    public string Server { get; set; } = null!;

    /// <summary>
    /// Gets or sets the PostgreSQL schema name for database operations.
    /// This schema is automatically added to the connection string's Search Path parameter.
    /// </summary>
    public string Schema { get; set; } = null!;

    /// <summary>
    /// Gets or sets the PostgreSQL database name to connect to.
    /// Used directly in the PostgreSQL connection string as the Database parameter.
    /// </summary>
    public string Database { get; set; } = null!;

    /// <summary>
    /// Gets or sets the optional PostgreSQL server port number.
    /// If not specified, the PostgreSQL default port (5432) will be used.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Gets or sets the database username for authentication.
    /// The requirement and usage of this property depends on the selected <see cref="Mode"/>:
    /// - Config mode: Required
    /// - StaticSecret mode: Required
    /// - DynamicSecret mode: Ignored (username is dynamically generated)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the database password for authentication.
    /// This property is only used in Config mode and should be avoided in production environments.
    /// For StaticSecret and DynamicSecret modes, credentials are retrieved from the secret manager.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the secret parameters dictionary for the Reader role.
    /// Used in StaticSecret and DynamicSecret modes to pass provider-specific parameters to IPvNugsSecretManager.
    /// The dictionary is passed as-is to the secret manager; keys and values depend on your provider implementation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Provider-Specific Examples:</strong></para>
    /// <para>HashiCorp Vault: { "mountPoint": "database", "role": "myapp-reader" }</para>
    /// <para>Azure Key Vault: { "name": "myapp-postgres-reader" }</para>
    /// <para>Environment Variables: { "name": "MYAPP_DB_READER_PASSWORD" }</para>
    /// <para>Consult your secret manager provider's documentation for required parameter keys.</para>
    /// </remarks>
    public Dictionary<string, string>? ReaderSecretParams { get; set; }
    
    /// <summary>
    /// Gets or sets the secret parameters dictionary for the Application role.
    /// Used in StaticSecret and DynamicSecret modes to pass provider-specific parameters to IPvNugsSecretManager.
    /// The dictionary is passed as-is to the secret manager; keys and values depend on your provider implementation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Provider-Specific Examples:</strong></para>
    /// <para>HashiCorp Vault: { "mountPoint": "database", "role": "myapp-application" }</para>
    /// <para>Azure Key Vault: { "name": "myapp-postgres-application" }</para>
    /// <para>Environment Variables: { "name": "MYAPP_DB_APP_PASSWORD" }</para>
    /// <para>Consult your secret manager provider's documentation for required parameter keys.</para>
    /// </remarks>
    public Dictionary<string, string>? ApplicationSecretParams { get; set; }
    
    /// <summary>
    /// Gets or sets the secret parameters dictionary for the Owner role.
    /// Used in StaticSecret and DynamicSecret modes to pass provider-specific parameters to IPvNugsSecretManager.
    /// The dictionary is passed as-is to the secret manager; keys and values depend on your provider implementation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Provider-Specific Examples:</strong></para>
    /// <para>HashiCorp Vault: { "mountPoint": "database", "role": "myapp-owner" }</para>
    /// <para>Azure Key Vault: { "name": "myapp-postgres-owner" }</para>
    /// <para>Environment Variables: { "name": "MYAPP_DB_OWNER_PASSWORD" }</para>
    /// <para>Consult your secret manager provider's documentation for required parameter keys.</para>
    /// </remarks>
    public Dictionary<string, string>? OwnerSecretParams { get; set; }
    
    /// <summary>
    /// Gets or sets the optional timezone setting for database connections.
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// Gets or sets the optional command timeout in seconds for database operations.
    /// </summary>
    public int? TimeoutInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the warning tolerance in minutes before dynamic secret expiration.
    /// When a dynamic secret is within this time window before expiration, a warning will be logged.
    /// Only applicable in DynamicSecret mode. Default value is typically 30 minutes.
    /// </summary>
    /// <remarks>
    /// This allows you to receive advance warning that credentials are approaching expiration,
    /// giving you time to investigate potential renewal issues before they become critical.
    /// </remarks>
    public int? ExpirationWarningToleranceInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the error tolerance in minutes before dynamic secret expiration.
    /// When a dynamic secret is within this time window before expiration, an exception will be thrown to prevent using nearly-expired credentials.
    /// Only applicable in DynamicSecret mode. Default value is typically 5 minutes.
    /// </summary>
    /// <remarks>
    /// This prevents the application from using credentials that might expire mid-transaction,
    /// forcing an early refresh instead. The error threshold should always be smaller than the warning threshold.
    /// </remarks>
    public int? ExpirationErrorToleranceInMinutes { get; set; }
}
