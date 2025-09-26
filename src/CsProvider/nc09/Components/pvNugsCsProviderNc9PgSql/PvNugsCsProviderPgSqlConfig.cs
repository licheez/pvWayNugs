// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace pvNugsCsProviderNc9PgSql;

/// <summary>
/// Configuration class for PostgreSQL connection string provider settings.
/// Contains all necessary parameters for database connections and credential management across different operational modes.
/// Property requirements vary based on the selected <see cref="Mode"/>.
/// </summary>
/// <remarks>
/// <para>This configuration class supports three operational modes with different parameter requirements:</para>
/// <list type="bullet">
/// <item><description><c>Config Mode:</c> All database connection properties plus <see cref="Username"/> are required. <see cref="Password"/> is optional.</description></item>
/// <item><description><c>StaticSecret Mode:</c> All database connection properties plus <see cref="Username"/> and <see cref="SecretName"/> are required.</description></item>
/// <item><description><c>DynamicSecret Mode:</c> All database connection properties plus <see cref="SecretName"/> are required. <see cref="Username"/> is ignored.</description></item>
/// </list>
/// <para>The configuration is typically loaded from appsettings.json using the section name <c>"PvNugsCsProviderPgSqlConfig"</c>.</para>
/// </remarks>
public class PvNugsCsProviderPgSqlConfig
{
    /// <summary>
    /// Gets the configuration section name used for loading settings from configuration files.
    /// Use this value when configuring the options pattern in dependency injection.
    /// </summary>
    /// <value>The string "PvNugsCsProviderPgSqlConfig"</value>
    /// <example>
    /// <code>
    /// services.Configure&lt;PvNugsCsProviderPgSqlConfig&gt;(
    ///     configuration.GetSection(PvNugsCsProviderPgSqlConfig.Section));
    /// </code>
    /// </example>
    public const string Section = nameof(PvNugsCsProviderPgSqlConfig);

    /// <summary>
    /// Gets or sets the operational mode for credential management.
    /// This property determines which other configuration properties are required and how credentials are retrieved.
    /// </summary>
    /// <value>A <see cref="CsProviderModeEnu"/> value specifying the credential management strategy.</value>
    /// <remarks>
    /// <para><c>Required:</c> Always required for all modes.</para>
    /// <para>This setting determines the provider's behavior and which constructor to use when creating the provider instance.</para>
    /// </remarks>
    public CsProviderModeEnu Mode { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL server hostname or IP address.
    /// </summary>
    /// <value>The server address for database connections (e.g., "localhost", "db.example.com", "192.168.1.100").</value>
    /// <remarks>
    /// <para><c>Required:</c> Always required for all modes.</para>
    /// <para>This value is used directly in the PostgreSQL connection string as the Server parameter.</para>
    /// </remarks>
    public string Server { get; set; } = null!;

    /// <summary>
    /// Gets or sets the PostgreSQL schema name for database operations.
    /// This schema is automatically added to the connection string's Search Path parameter.
    /// </summary>
    /// <value>The schema name that will be used as the default search path (e.g., "public", "app_schema").</value>
    /// <remarks>
    /// <para><c>Required:</c> Always required for all modes.</para>
    /// <para>The schema is automatically included in the PostgreSQL Search Path, making it the default schema for unqualified table references.</para>
    /// </remarks>
    public string Schema { get; set; }= null!;

    /// <summary>
    /// Gets or sets the PostgreSQL database name to connect to.
    /// </summary>
    /// <value>The target database name (e.g., "myapp_db", "production_db").</value>
    /// <remarks>
    /// <para><c>Required:</c> Always required for all modes.</para>
    /// <para>This value is used directly in the PostgreSQL connection string as the Database parameter.</para>
    /// </remarks>
    public string Database { get; set; }= null!;

    /// <summary>
    /// Gets or sets the optional PostgreSQL server port number.
    /// If not specified, the PostgreSQL default port (5432) will be used.
    /// </summary>
    /// <value>The port number for database connections, or null to use the default port.</value>
    /// <remarks>
    /// <para><c>Required:</c> Optional for all modes.</para>
    /// <para>When null, the PostgreSQL driver will use the standard port 5432. Specify this only when using non-standard ports.</para>
    /// </remarks>
    public int? Port { get; set; }
    
    /// <summary>
    /// Gets or sets the optional timezone setting for database connections.
    /// </summary>
    /// <value>The timezone string (e.g., "UTC", "America/New_York"), or null to use the server's default timezone.</value>
    /// <remarks>
    /// <para><c>Required:</c> Optional for all modes.</para>
    /// <para>When specified, this value is added to the connection string as the TimeZone parameter.</para>
    /// </remarks>
    public string? Timezone { get; set; }
    
    /// <summary>
    /// Gets or sets the optional command timeout in seconds for database operations.
    /// </summary>
    /// <value>The timeout duration in seconds, or null to use the driver's default timeout.</value>
    /// <remarks>
    /// <para><c>Required:</c> Optional for all modes.</para>
    /// <para>When specified, this value is added to the connection string as the CommandTimeout parameter.</para>
    /// </remarks>
    public int? TimeoutInSeconds { get; set; }
    
    /// <summary>
    /// Gets or sets the database username for authentication.
    /// The requirement and usage of this property depends on the selected <see cref="Mode"/>.
    /// </summary>
    /// <value>The username for database authentication, or null when not required.</value>
    /// <remarks>
    /// <para><c>Config Mode:</c> Required - used directly for database authentication.</para>
    /// <para><c>StaticSecret Mode:</c> Required - used for database authentication while passwords come from secret manager.</para>
    /// <para><c>DynamicSecret Mode:</c> Not used - both username and password are dynamically generated by the secret manager.</para>
    /// </remarks>
    public string? Username { get; set; }
    
    /// <summary>
    /// Gets or sets the database password for authentication.
    /// This property is only used in Config mode and should be avoided in production environments.
    /// </summary>
    /// <value>The password for database authentication, or null when not used.</value>
    /// <remarks>
    /// <para><c>Config Mode:</c> Optional - can be null for password-less authentication or when using integrated security.</para>
    /// <para><c>StaticSecret Mode:</c> Not used - passwords are retrieved from the secret manager.</para>
    /// <para><c>DynamicSecret Mode:</c> Not used - passwords are dynamically generated by the secret manager.</para>
    /// <para><c>Security Warning:</c> Storing passwords in configuration is not recommended for production environments.</para>
    /// </remarks>
    public string? Password { get; set; }
    
    /// <summary>
    /// Gets or sets the base secret name used for credential retrieval from secret managers.
    /// The actual secret name queried follows the pattern: <c>{SecretName}-{Role}</c>.
    /// </summary>
    /// <value>The base name for secrets stored in the secret manager, or null when not using secret managers.</value>
    /// <remarks>
    /// <para><c>Config Mode:</c> Not used - credentials come from configuration.</para>
    /// <para><c>StaticSecret Mode:</c> Required - used to construct secret names for password retrieval.</para>
    /// <para><c>DynamicSecret Mode:</c> Required - used to construct secret names for dynamic credential generation.</para>
    /// <para><c>Secret Naming Pattern:</c> The provider appends the SQL role name to create the full secret name.</para>
    /// <para>Example: If SecretName is "myapp-db", the provider will query for "myapp-db-Reader", "myapp-db-Application", etc.</para>
    /// </remarks>
    public string? SecretName { get; set; }

    /// <summary>
    /// When a dynamic secret is within this time window before expiration, a warning will be logged.
    /// </summary>
    /// <value>The warning tolerance in minutes, or null to use the default value of 30 minutes.</value>
    /// <remarks>
    /// <para><c>Config Mode:</c> Not used - no expiration for configuration-based credentials.</para>
    /// <para><c>StaticSecret Mode:</c> Not used - static secrets do not expire.</para>
    /// <para><c>DynamicSecret Mode:</c> Optional - used to determine when to log expiration warnings.</para>
    /// <para>When null, the default warning tolerance of 30 minutes is applied.</para>
    /// <para>This should be set higher than <see cref="ExpirationErrorToleranceInMinutes"/> to provide appropriate warning before errors.</para>
    /// </remarks>
    public int? ExpirationWarningToleranceInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the error tolerance in minutes before secret expiration.
    /// When a dynamic secret is within this time window before expiration, an error will be thrown.
    /// </summary>
    /// <value>The error tolerance in minutes, or null to use the default value of 5 minutes.</value>
    /// <remarks>
    /// <para><c>Config Mode:</c> Not used - no expiration for configuration-based credentials.</para>
    /// <para><c>StaticSecret Mode:</c> Not used - static secrets do not expire.</para>
    /// <para><c>DynamicSecret Mode:</c> Optional - used to determine when to throw expiration errors.</para>
    /// <para>When null, the default error tolerance of 5 minutes is applied.</para>
    /// <para>This should be set lower than <see cref="ExpirationWarningToleranceInMinutes"/> to allow warnings before errors occur.</para>
    /// </remarks>
    public int? ExpirationErrorToleranceInMinutes { get; set; }

}

