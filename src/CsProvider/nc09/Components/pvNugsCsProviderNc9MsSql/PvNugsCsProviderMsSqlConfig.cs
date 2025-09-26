// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace pvNugsCsProviderNc9MsSql;

/// <summary>
/// Configuration class for MsSQL connection string provider settings.
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
/// <para>The configuration is typically loaded from appsettings.json using the section name <c>"PvNugsCsProviderMsSqlConfig"</c>.</para>
/// </remarks>
/// <seealso cref="PvNugsCsProviderMsSqlConfigRow"/>
public class PvNugsCsProviderMsSqlConfig
{
    /// <summary>
    /// Gets the configuration section name used for loading settings from configuration files.
    /// Use this value when configuring the options pattern in dependency injection.
    /// </summary>
    /// <value>The string "PvNugsCsProviderMsSqlConfig"</value>
    /// <example>
    /// <code>
    /// services.Configure&lt;PvNugsCsProviderMsSqlConfig&gt;(
    ///     configuration.GetSection(PvNugsCsProviderMsSqlConfig.Section));
    /// </code>
    /// </example>
    public const string Section = nameof(PvNugsCsProviderMsSqlConfig);

    /// <summary>
    /// Gets or sets the list of configuration rows for multi-database support.
    /// Each row represents a separate MsSQL connection configuration.
    /// For backward compatibility, the flat properties map to the first row.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public IEnumerable<PvNugsCsProviderMsSqlConfigRow>? Rows { get; set; } = [];

    // --- Centralized row accessor for backward compatibility ---
    /// <summary>
    /// Gets the first configuration row, ensuring the Rows collection is initialized and mutable.
    /// Used internally to provide backward compatibility for flat property accessors.
    /// </summary>
    private PvNugsCsProviderMsSqlConfigRow Row0
    {
        get
        {
            if (Rows == null || !Rows.Any())
            {
                Rows = new List<PvNugsCsProviderMsSqlConfigRow> { new() };
            }
            else if (Rows is not List<PvNugsCsProviderMsSqlConfigRow>)
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
    /// <value>A <see cref="CsProviderModeEnu"/> value specifying the credential management strategy.</value>
    public CsProviderModeEnu Mode
    {
        get => Row0.Mode;
        set => Row0.Mode = value;
    }

    /// <summary>
    /// Gets or sets the MsSQL server hostname or IP address.
    /// Used directly in the MsSQL connection string as the Server parameter.
    /// </summary>
    /// <value>The server address for database connections (e.g., "localhost", "db.example.com"). Never null.</value>
    public string Server
    {
        get => Row0.Server;
        set => Row0.Server = value;
    }

    /// <summary>
    /// Gets or sets the MsSQL database name to connect to.
    /// Used directly in the MsSQL connection string as the Database parameter.
    /// </summary>
    /// <value>The target database name (e.g., "myapp_db", "production_db"). Never null.</value>
    public string Database
    {
        get => Row0.Database;
        set => Row0.Database = value;
    }

    /// <summary>
    /// Gets or sets the optional MsSQL server port number.
    /// If not specified, the MsSQL default port (5432) will be used.
    /// </summary>
    /// <value>The port number for database connections, or null to use the default port.</value>
    public int? Port
    {
        get => Row0.Port;
        set => Row0.Port = value;
    }

    /// <summary>
    /// Gets or sets the optional command timeout in seconds for database operations.
    /// When specified, this value is added to the connection string as the CommandTimeout parameter.
    /// </summary>
    /// <value>The timeout duration in seconds, or null to use the driver's default timeout.</value>
    public int? TimeoutInSeconds
    {
        get => Row0.TimeoutInSeconds;
        set => Row0.TimeoutInSeconds = value;
    }

    /// <summary>
    /// Gets or sets the database username for authentication.
    /// The requirement and usage of this property depends on the selected <see cref="Mode"/>.
    /// </summary>
    /// <value>The username for database authentication, or null when not required.</value>
    public string? Username
    {
        get => Row0.Username;
        set => Row0.Username = value;
    }

    /// <summary>
    /// Gets or sets the application name used in connection strings for identification and monitoring purposes.
    /// If null or empty, defaults to the entry assembly name.
    /// </summary>
    /// <value>The application name to use in connection strings, or null to use the default.</value>
    public string? ApplicationName
    {
        get => Row0.ApplicationName;
        set => Row0.ApplicationName = value;
    }

    /// <summary>
    /// Gets or sets the database password for authentication.
    /// Only used in Config mode; not recommended for production environments.
    /// </summary>
    /// <value>The password for database authentication, or null when not used.</value>
    public string? Password
    {
        get => Row0.Password;
        set => Row0.Password = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the connection should use Windows Integrated Security (Windows Authentication)
    /// instead of SQL Server authentication for database connections. When enabled, the current Windows user's credentials
    /// are used for authentication, eliminating the need for explicit username and password credentials.
    /// </summary>
    /// <value>
    /// <c>true</c> to use Windows Integrated Security (Trusted Connection) for authentication;
    /// <c>false</c> to use SQL Server authentication with explicit credentials. The default value is <c>false</c>.
    /// </value>
    public bool? UseIntegratedSecurity
    {
        get => Row0.UseIntegratedSecurity;
        set => Row0.UseIntegratedSecurity = value ?? false;
    }

    /// <summary>
    /// Gets or sets the base secret name used for credential retrieval from secret managers.
    /// The actual secret name queried follows the pattern: <c>{SecretName}-{Role}</c>.
    /// </summary>
    /// <value>The base name for secrets stored in the secret manager, or null when not using secret managers.</value>
    public string? SecretName
    {
        get => Row0.SecretName;
        set => Row0.SecretName = value;
    }
}