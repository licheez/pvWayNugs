// ReSharper disable UnusedAutoPropertyAccessor.Global

using pvNugsCsProviderNc9Abstractions;

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
    /// Gets or sets the MsSQL server hostname or IP address.
    /// </summary>
    /// <value>The server address for database connections (e.g., "localhost", "db.example.com", "192.168.1.100").</value>
    /// <remarks>
    /// <para><c>Required:</c> Always required for all modes.</para>
    /// <para>This value is used directly in the MsSQL connection string as the Server parameter.</para>
    /// </remarks>
    public string Server { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the MsSQL database name to connect to.
    /// </summary>
    /// <value>The target database name (e.g., "myapp_db", "production_db").</value>
    /// <remarks>
    /// <para><c>Required:</c> Always required for all modes.</para>
    /// <para>This value is used directly in the MsSQL connection string as the Database parameter.</para>
    /// </remarks>
    public string Database { get; set; }= null!;
    
    /// <summary>
    /// Gets or sets the optional MsSQL server port number.
    /// If not specified, the MsSQL default port (5432) will be used.
    /// </summary>
    /// <value>The port number for database connections, or null to use the default port.</value>
    /// <remarks>
    /// <para><c>Required:</c> Optional for all modes.</para>
    /// <para>When null, the MsSQL driver will use the standard port 5432. Specify this only when using non-standard ports.</para>
    /// </remarks>
    public int? Port { get; set; }
    
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
    /// Gets or sets the application name used in connection strings for identification and monitoring purposes.
    /// When using Windows Authentication, this is combined with the SQL role to create unique application names
    /// for better tracking and auditing (e.g., "MyApp-Reader", "MyApp-Application").
    /// </summary>
    /// <value>
    /// The application name to use in connection strings. If null or empty, defaults to the entry assembly name.
    /// </value>
    /// <remarks>
    /// <para><c>Usage:</c> This value appears in SQL Server's sys.dm_exec_sessions and connection logs</para>
    /// <para><c>Default Behavior:</c> When not specified, uses Assembly.GetEntryAssembly()?.GetName().Name</para>
    /// <para><c>Best Practice:</c> Use descriptive names like "OrderService", "PaymentAPI", "ReportingApp"</para>
    /// </remarks>
    public string? ApplicationName { get; set; }
    
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
    /// Gets or sets a value indicating whether the connection should use Windows Integrated Security (Windows Authentication) 
    /// instead of SQL Server authentication for database connections. When enabled, the current Windows user's credentials 
    /// are used for authentication, eliminating the need for explicit username and password credentials.
    /// </summary>
    /// <value>
    /// <c>true</c> to use Windows Integrated Security (Trusted Connection) for authentication; 
    /// <c>false</c> to use SQL Server authentication with explicit credentials. The default value is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para><c>Authentication Modes:</c></para>
    /// <para>This property determines the authentication mechanism used for SQL Server connections:</para>
    /// <list type="bullet">
    /// <item><description><c>true (Integrated Security):</c> Uses Windows Authentication with the current user's Windows credentials</description></item>
    /// <item><description><c>false (SQL Authentication):</c> Uses SQL Server authentication requiring explicit username and password</description></item>
    /// </list>
    /// 
    /// <para><c>Connection String Impact:</c></para>
    /// <para>When <c>true</c>, this property affects the generated connection string by:</para>
    /// <list type="bullet">
    /// <item><description>Adding <c>Integrated Security=true</c> or <c>Trusted_Connection=true</c> to the connection string</description></item>
    /// <item><description>Omitting <c>User ID</c> and <c>Password</c> parameters from the connection string</description></item>
    /// <item><description>Potentially ignoring configured username and password values in all authentication modes</description></item>
    /// </list>
    /// 
    /// <para><c>Security Considerations:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Enhanced Security:</c> Eliminates the need to store or transmit database passwords</description></item>
    /// <item><description><c>Single Sign-On:</c> Provides seamless authentication using existing Windows credentials</description></item>
    /// <item><description><c>Credential Management:</c> Windows handles credential management and rotation automatically</description></item>
    /// <item><description><c>Network Security:</c> Supports Kerberos authentication for secure network communication</description></item>
    /// </list>
    /// 
    /// <para><c>Environment Requirements:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Windows Environment:</c> Both client application and SQL Server must be running on Windows</description></item>
    /// <item><description><c>Domain Environment:</c> Typically requires Active Directory domain membership for both client and server</description></item>
    /// <item><description><c>Service Account:</c> Application service account must have appropriate SQL Server login and permissions</description></item>
    /// <item><description><c>Network Configuration:</c> May require specific firewall and network configuration for Kerberos</description></item>
    /// </list>
    /// 
    /// <para><c>Mode Compatibility:</c></para>
    /// <para>This property interacts differently with each <see cref="CsProviderModeEnu"/> value:</para>
    /// <list type="bullet">
    /// <item><description><c>Config Mode:</c> When true, overrides configured username/password settings</description></item>
    /// <item><description><c>StaticSecret Mode:</c> When true, bypasses secret manager password retrieval</description></item>
    /// <item><description><c>DynamicSecret Mode:</c> When true, bypasses dynamic credential generation entirely</description></item>
    /// </list>
    /// 
    /// <para><c>Performance Considerations:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Authentication Speed:</c> May be faster than SQL authentication in domain environments</description></item>
    /// <item><description><c>Connection Pooling:</c> Connections are pooled per Windows user identity</description></item>
    /// <item><description><c>Credential Caching:</c> Windows handles credential caching and refresh automatically</description></item>
    /// </list>
    /// 
    /// <para><c>Deployment Scenarios:</c></para>
    /// <list type="bullet">
    /// <item><description><c>On-Premises Applications:</c> Ideal for corporate environments with Active Directory</description></item>
    /// <item><description><c>Windows Services:</c> Excellent choice for Windows services running under service accounts</description></item>
    /// <item><description><c>Desktop Applications:</c> Provides seamless user experience in domain environments</description></item>
    /// <item><description><c>Cloud Limitations:</c> May not be suitable for cloud-based applications or Linux deployments</description></item>
    /// </list>
    /// 
    /// <para><c>Troubleshooting:</c></para>
    /// <para>Common issues when using Integrated Security:</para>
    /// <list type="bullet">
    /// <item><description><c>Login Failed:</c> Verify the Windows account has SQL Server login permissions</description></item>
    /// <item><description><c>Double-Hop Issues:</c> May require Kerberos delegation for multi-tier applications</description></item>
    /// <item><description><c>SPN Configuration:</c> Service Principal Names may need configuration for Kerberos authentication</description></item>
    /// <item><description><c>Firewall Issues:</c> Ensure ports 1433 (SQL) and 1434 (SQL Browser) are accessible</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para>Configuration for Windows Authentication in appsettings.json:</para>
    /// <code>
    /// {
    ///   "PvNugsCsProviderMsSqlConfig": {
    ///     "Mode": "Config",
    ///     "Server": "localhost\\SQLEXPRESS",
    ///     "Database": "MyApplication",
    ///     "UseIntegratedSecurity": true,
    ///     // Username and Password are ignored when UseIntegratedSecurity is true
    ///     "Username": "",
    ///     "Password": ""
    ///   }
    /// }
    /// </code>
    /// 
    /// <para>Using integrated security with different authentication modes:</para>
    /// <code>
    /// // Windows Authentication takes precedence regardless of mode
    /// var config = new PvNugsCsProviderMsSqlConfig
    /// {
    ///     Mode = CsProviderModeEnu.DynamicSecret,
    ///     Server = "myserver.domain.com",
    ///     Database = "ProductionDB",
    ///     UseIntegratedSecurity = true,  // Dynamic credentials will be bypassed
    ///     SecretName = "app-database"    // This will be ignored
    /// };
    /// </code>
    /// 
    /// <para>Checking integrated security status at runtime:</para>
    /// <code>
    /// public class DatabaseService
    /// {
    ///     private readonly IPvNugsMsSqlCsProvider _csProvider;
    /// 
    ///     public DatabaseService(IPvNugsMsSqlCsProvider csProvider)
    ///     {
    ///         _csProvider = csProvider;
    ///     }
    /// 
    ///     public async Task&lt;string&gt; GetConnectionInfoAsync()
    ///     {
    ///         var connectionString = await _csProvider.GetConnectionStringAsync(SqlRoleEnu.Application);
    ///         
    ///         if (_csProvider.UseTrustedConnection)
    ///         {
    ///             // Using Windows Authentication
    ///             var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
    ///             _logger.LogInformation("Using Windows Authentication for user: {User}", currentUser);
    ///         }
    ///         else
    ///         {
    ///             // Using SQL Authentication
    ///             var username = _csProvider.GetUsername(SqlRoleEnu.Application);
    ///             _logger.LogInformation("Using SQL Authentication for user: {Username}", username);
    ///         }
    ///         
    ///         return connectionString;
    ///     }
    /// }
    /// </code>
    /// 
    /// <para>Service account configuration example:</para>
    /// <code>
    /// // When deploying as a Windows Service with integrated security:
    /// // 1. Create a dedicated service account in Active Directory
    /// // 2. Grant SQL Server login permissions to the service account
    /// // 3. Configure the Windows Service to run under the service account
    /// // 4. Set UseIntegratedSecurity = true in configuration
    /// 
    /// // SQL Server setup for service account:
    /// /*
    /// CREATE LOGIN [DOMAIN\MyAppServiceAccount] FROM WINDOWS;
    /// CREATE USER [DOMAIN\MyAppServiceAccount] FOR LOGIN [DOMAIN\MyAppServiceAccount];
    /// ALTER ROLE [db_datareader] ADD MEMBER [DOMAIN\MyAppServiceAccount];  -- For Reader role
    /// ALTER ROLE [db_datawriter] ADD MEMBER [DOMAIN\MyAppServiceAccount];  -- For Application role
    /// ALTER ROLE [db_owner] ADD MEMBER [DOMAIN\MyAppServiceAccount];       -- For Owner role
    /// */
    /// </code>
    /// </example>
    /// <seealso cref="CsProviderModeEnu"/>
    /// <seealso cref="IPvNugsMsSqlCsProvider.UseTrustedConnection"/>
    /// <seealso cref="IPvNugsMsSqlCsProvider.UseDynamicCredentials"/>
    /// <seealso href="https://docs.microsoft.com/en-us/sql/relational-databases/security/choose-an-authentication-mode">SQL Server Authentication Modes</seealso>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/authentication-in-sql-server">Authentication in SQL Server</seealso>
    public bool UseIntegratedSecurity { get; set; } = false;
    
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
}
