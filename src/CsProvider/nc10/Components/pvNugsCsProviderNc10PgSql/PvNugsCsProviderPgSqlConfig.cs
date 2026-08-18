// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace pvNugsCsProviderNc10PgSql;

/// <summary>
/// Configuration class for PostgreSQL connection string provider settings.
/// Supports multi-database configuration via the Rows property.
/// Property requirements vary based on the selected <see cref="Mode"/>.
/// </summary>
/// <remarks>
/// <para>This configuration class supports three operational modes with different parameter requirements:</para>
/// <list type="bullet">
/// <item><description><c>Config Mode:</c> All database connection properties plus <see cref="Username"/> are required. <see cref="Password"/> is optional.</description></item>
/// <item><description><c>StaticSecret Mode:</c> All database connection properties plus <see cref="Username"/> and role-specific SecretParams (ReaderSecretParams, ApplicationSecretParams, OwnerSecretParams) are required. Requires IPvNugsSecretManager.</description></item>
/// <item><description><c>DynamicSecret Mode:</c> All database connection properties plus role-specific SecretParams are required. <see cref="Username"/> is ignored. Requires IPvNugsSecretManager with SupportsDatabaseSecrets = true.</description></item>
/// </list>
/// <para>The configuration is typically loaded from appsettings.json using the section name <c>"PvNugsCsProviderPgSqlConfig"</c>.</para>
/// </remarks>
public class PvNugsCsProviderPgSqlConfig
{
    /// <summary>
    /// Gets the configuration section name used for loading settings from configuration files.
    /// Use this value when configuring the options pattern in dependency injection.
    /// </summary>
    public const string Section = nameof(PvNugsCsProviderPgSqlConfig);

    /// <summary>
    /// Gets or sets the list of configuration rows for multi-database support.
    /// Each row represents a separate PostgreSQL connection configuration.
    /// For backward compatibility, the flat properties map to the first row.
    /// </summary>
    public IEnumerable<PvNugsCsProviderPgSqlConfigRow>? Rows { get; set; } = [];

    // --- Centralized row accessor for backward compatibility ---
    /// <summary>
    /// Gets the first configuration row, ensuring the Rows collection is initialized and mutable.
    /// Used internally to provide backward compatibility for flat property accessors.
    /// </summary>
    private PvNugsCsProviderPgSqlConfigRow Row0
    {
        get
        {
            if (Rows == null || !Rows.Any())
            {
                Rows = new List<PvNugsCsProviderPgSqlConfigRow> { new() };
            }
            else if (Rows is not List<PvNugsCsProviderPgSqlConfigRow>)
            {
                Rows = Rows.ToList();
            }
            return Rows!.First();
        }
    }

    /// <summary>
    /// Gets or sets the operational mode for credential management.
    /// Determines which other configuration properties are required and how credentials are retrieved.
    /// </summary>
    public CsProviderModeEnu Mode
    {
        get => Row0.Mode;
        set => Row0.Mode = value;
    }

    /// <summary>
    /// Gets or sets the PostgreSQL server hostname or IP address.
    /// Used directly in the PostgreSQL connection string as the Server parameter.
    /// </summary>
    public string Server
    {
        get => Row0.Server;
        set => Row0.Server = value;
    }

    /// <summary>
    /// Gets or sets the PostgreSQL schema name for database operations.
    /// This schema is automatically added to the connection string's Search Path parameter.
    /// </summary>
    public string Schema
    {
        get => Row0.Schema;
        set => Row0.Schema = value;
    }

    /// <summary>
    /// Gets or sets the PostgreSQL database name to connect to.
    /// Used directly in the PostgreSQL connection string as the Database parameter.
    /// </summary>
    public string Database
    {
        get => Row0.Database;
        set => Row0.Database = value;
    }

    /// <summary>
    /// Gets or sets the optional PostgreSQL server port number.
    /// If not specified, the PostgreSQL default port (5432) will be used.
    /// </summary>
    public int? Port
    {
        get => Row0.Port;
        set => Row0.Port = value;
    }

    /// <summary>
    /// Gets or sets the optional timezone setting for database connections.
    /// </summary>
    public string? Timezone
    {
        get => Row0.Timezone;
        set => Row0.Timezone = value;
    }

    /// <summary>
    /// Gets or sets the optional command timeout in seconds for database operations.
    /// </summary>
    public int? TimeoutInSeconds
    {
        get => Row0.TimeoutInSeconds;
        set => Row0.TimeoutInSeconds = value;
    }

    /// <summary>
    /// Gets or sets the database username for authentication.
    /// The requirement and usage of this property depends on the selected <see cref="Mode"/>:
    /// - Config mode: Required
    /// - StaticSecret mode: Required
    /// - DynamicSecret mode: Ignored (username is dynamically generated)
    /// </summary>
    public string? Username
    {
        get => Row0.Username;
        set => Row0.Username = value;
    }

    /// <summary>
    /// Gets or sets the database password for authentication.
    /// This property is only used in Config mode and should be avoided in production environments.
    /// For StaticSecret and DynamicSecret modes, credentials are retrieved from the secret manager.
    /// </summary>
    public string? Password
    {
        get => Row0.Password;
        set => Row0.Password = value;
    }

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
    public Dictionary<string, string>? ReaderSecretParams
    {
        get => Row0.ReaderSecretParams;
        set => Row0.ReaderSecretParams = value;
    }

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
    public Dictionary<string, string>? ApplicationSecretParams
    {
        get => Row0.ApplicationSecretParams;
        set => Row0.ApplicationSecretParams = value;
    }
    
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
    public Dictionary<string, string>? OwnerSecretParams
    {
        get => Row0.OwnerSecretParams;
        set => Row0.OwnerSecretParams = value;
    }
    
    /// <summary>
    /// Gets or sets the warning tolerance in minutes before dynamic secret expiration.
    /// When a dynamic secret is within this time window before expiration, a warning will be logged.
    /// Only applicable in DynamicSecret mode. Default value is typically 30 minutes.
    /// </summary>
    /// <remarks>
    /// This allows you to receive advance warning that credentials are approaching expiration,
    /// giving you time to investigate potential renewal issues before they become critical.
    /// </remarks>
    public int? ExpirationWarningToleranceInMinutes
    {
        get => Row0.ExpirationWarningToleranceInMinutes;
        set => Row0.ExpirationWarningToleranceInMinutes = value;
    }

    /// <summary>
    /// Gets or sets the error tolerance in minutes before dynamic secret expiration.
    /// When a dynamic secret is within this time window before expiration, an exception will be thrown to prevent using nearly-expired credentials.
    /// Only applicable in DynamicSecret mode. Default value is typically 5 minutes.
    /// </summary>
    /// <remarks>
    /// This prevents the application from using credentials that might expire mid-transaction,
    /// forcing an early refresh instead. The error threshold should always be smaller than the warning threshold.
    /// </remarks>
    public int? ExpirationErrorToleranceInMinutes
    {
        get => Row0.ExpirationErrorToleranceInMinutes;
        set => Row0.ExpirationErrorToleranceInMinutes = value;
    }
}
