// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace pvNugsCsProviderNc9PgSql;

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
    /// The requirement and usage of this property depends on the selected <see cref="Mode"/>.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the database password for authentication.
    /// This property is only used in Config mode and should be avoided in production environments.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the base secret name used for credential retrieval from secret managers.
    /// The actual secret name queried follows the pattern: <c>{SecretName}-{Role}</c>.
    /// </summary>
    public string? SecretName { get; set; }

    /// <summary>
    /// Gets or sets the optional timezone setting for database connections.
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// Gets or sets the optional command timeout in seconds for database operations.
    /// </summary>
    public int? TimeoutInSeconds { get; set; }

    /// <summary>
    /// When a dynamic secret is within this time window before expiration, a warning will be logged.
    /// </summary>
    public int? ExpirationWarningToleranceInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the error tolerance in minutes before secret expiration.
    /// When a dynamic secret is within this time window before expiration, an error will be thrown.
    /// </summary>
    public int? ExpirationErrorToleranceInMinutes { get; set; }
}
