// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace pvNugsCsProviderNc9PgSql;

/// <summary>
/// Configuration class for PostgreSQL connection string provider settings.
/// Supports multi-database configuration via the Rows property.
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
    /// The requirement and usage of this property depends on the selected <see cref="Mode"/>.
    /// </summary>
    public string? Username
    {
        get => Row0.Username;
        set => Row0.Username = value;
    }

    /// <summary>
    /// Gets or sets the database password for authentication.
    /// This property is only used in Config mode and should be avoided in production environments.
    /// </summary>
    public string? Password
    {
        get => Row0.Password;
        set => Row0.Password = value;
    }

    /// <summary>
    /// Gets or sets the base secret name used for credential retrieval from secret managers.
    /// The actual secret name queried follows the pattern: <c>{SecretName}-{Role}</c>.
    /// </summary>
    public string? SecretName
    {
        get => Row0.SecretName;
        set => Row0.SecretName = value;
    }

    /// <summary>
    /// When a dynamic secret is within this time window before expiration, a warning will be logged.
    /// </summary>
    public int? ExpirationWarningToleranceInMinutes
    {
        get => Row0.ExpirationWarningToleranceInMinutes;
        set => Row0.ExpirationWarningToleranceInMinutes = value;
    }

    /// <summary>
    /// Gets or sets the error tolerance in minutes before secret expiration.
    /// When a dynamic secret is within this time window before expiration, an error will be thrown.
    /// </summary>
    public int? ExpirationErrorToleranceInMinutes
    {
        get => Row0.ExpirationErrorToleranceInMinutes;
        set => Row0.ExpirationErrorToleranceInMinutes = value;
    }
}
