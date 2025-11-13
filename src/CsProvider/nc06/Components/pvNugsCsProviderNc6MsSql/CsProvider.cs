using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc6Abstractions;
using pvNugsLoggerNc6Abstractions;
using pvNugsSecretManagerNc6Abstractions;
using SqlRoleEnu = pvNugsCsProviderNc6Abstractions.SqlRoleEnu;

namespace pvNugsCsProviderNc6MsSql;

/// <summary>
/// Provides MsSQL connection strings with role-based access control and multiple credential management modes.
/// This class supports three operational modes: Config (configuration-based), StaticSecret (secret manager with static secrets),
/// and DynamicSecret (secret manager with time-limited credentials).
/// Connection strings are cached per role and automatically refreshed when dynamic credentials expire.
/// </summary>
/// <remarks>
/// <para><c>Constructor Selection:</c></para>
/// <list type="bullet">
/// <item><description><c>Config Mode:</c> Use the primary constructor with logger and options only. Credentials are read from configuration.</description></item>
/// <item><description><c>StaticSecret Mode:</c> Use the constructor with <see cref="IPvNugsStaticSecretManager"/>. Passwords are retrieved from a secret manager using static secrets.</description></item>
/// <item><description><c>DynamicSecret Mode:</c> Use the constructor with <see cref="IPvNugsDynamicSecretManager"/>. Username/password pairs are dynamically generated with expiration times.</description></item>
/// </list>
/// <para><c>Secret Name Resolution:</c></para>
/// <para>For StaticSecret and DynamicSecret modes, the secret name is constructed as: <c>{SecretName}-{Role}</c></para>
/// <para>Where SecretName comes from configuration and Role is the SQL role (Owner, Application, or Reader).</para>
/// <para>Example: If SecretName is "myapp-db" and role is "Application", the secret manager will query for "myapp-db-Application".</para>
/// </remarks>
/// <seealso cref="IPvNugsMsSqlCsProvider"/>
public class CsProvider : IPvNugsMsSqlCsProvider
{
    private const string DefaultCsName = "Default";
    
    /// <summary>
    /// Represents a cached connection string entry with optional expiration support.
    /// Used internally to cache connection strings and track their validity for dynamic credentials.
    /// </summary>
    private sealed class CsEntry
    {
        /// <summary>
        /// Represents a cached connection string entry with optional expiration support.
        /// Used internally to cache connection strings and track their validity for dynamic credentials.
        /// </summary>
        public CsEntry(string username,
            string connectionString,
            DateTime? expirationDateUtc)
        {
            UserName = username;
            ConnectionString = connectionString;
            ExpirationDateUtc = expirationDateUtc;
        }

        /// <summary>
        /// Gets the username associated with this connection string entry.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Gets the complete connection string for database connections.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the UTC expiration date for this entry, if applicable.
        /// </summary>
        private DateTime? ExpirationDateUtc { get; }

        /// <summary>
        /// Gets a value indicating whether this connection string entry has expired.
        /// Returns false for entries without expiration dates (Config and StaticSecret modes).
        /// </summary>
        public bool IsExpired
        {
            get
            {
                if (!ExpirationDateUtc.HasValue) return false;
                return ExpirationDateUtc.Value < DateTime.UtcNow;
            }
        }
    }

    private readonly IPvNugsStaticSecretManager? _staticSecretManager;
    private readonly IPvNugsDynamicSecretManager? _dynamicSecretManager;

    private readonly ConcurrentDictionary<string, CsEntry> _csEntries = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private readonly IEnumerable<PvNugsCsProviderMsSqlConfigRow> 
        _configRows;

    private readonly IConsoleLoggerService _logger;

    private PvNugsCsProviderMsSqlConfigRow GetConfigRow(string connectionStringName)
    {
        var row = _configRows.FirstOrDefault(x =>
            string.Compare(x.Name, connectionStringName, StringComparison.InvariantCultureIgnoreCase) == 0);
        if (row != null) return row;
        var err = $"Connection string name '{connectionStringName}' not found in configuration";
        _logger.Log(err, SeverityEnu.Error);
        throw new PvNugsCsProviderException(err);
    }
    
    /// <summary>
    /// <inheritdoc cref="IPvNugsMsSqlCsProvider.UseTrustedConnection"/>
    /// </summary>
    public bool UseTrustedConnection => IsTrustedConnection(DefaultCsName);

    /// <summary>
    /// Determines if the specified connection string uses Windows Integrated Security (Trusted Connection).
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration row.</param>
    /// <returns>True if integrated security is enabled; otherwise, false.</returns>
    public bool IsTrustedConnection(string connectionStringName)
    {
        var row = GetConfigRow(connectionStringName);
        return row.UseIntegratedSecurity;
    }

    /// <summary>
    /// Gets a value indicating whether this provider uses dynamic credentials with expiration times.
    /// Returns true when configured for DynamicSecret mode, false for Config or StaticSecret modes.
    /// </summary>
    /// <inheritdoc cref="IPvNugsMsSqlCsProvider.UseDynamicCredentials"/>
    public bool UseDynamicCredentials => IsDynamicCredentials(DefaultCsName);

    /// <summary>
    /// Determines if the specified connection string uses dynamic credentials (DynamicSecret mode).
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration row.</param>
    /// <returns>True if dynamic credentials are used; otherwise, false.</returns>
    public bool IsDynamicCredentials(string connectionStringName)
    {
        var row = GetConfigRow(connectionStringName);
        return row.Mode == CsProviderModeEnu.DynamicSecret;
    }

    /// <summary>
    /// Gets the username for the specified database role from the current cache.
    /// </summary>
    /// <param name="role">The database role to get the username for.</param>
    /// <returns>The cached username for the role, or an empty string if no cached entry exists.</returns>
    /// <remarks>
    /// This method returns the currently cached username and does not trigger credential refresh.
    /// For Config and StaticSecret modes, this will be the configured username.
    /// For DynamicSecret mode, this will be the dynamically generated username from the last credential fetch.
    /// </remarks>
    public string GetUsername(SqlRoleEnu role) => GetUsername(DefaultCsName, role);

    /// <summary>
    /// Gets the username for the specified connection string and database role from the current cache.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration row.</param>
    /// <param name="role">The database role to get the username for.</param>
    /// <returns>The cached username for the role, or an empty string if no cached entry exists.</returns>
    public string GetUsername(string connectionStringName, SqlRoleEnu role)
    {
        var csEntryKey = $"{connectionStringName}-{role}";
        var exists = _csEntries.TryGetValue(csEntryKey, out var csEntry);
        return exists ? csEntry!.UserName : string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsProvider"/> class for StaticSecret mode.
    /// Use this constructor when credentials should be retrieved from a secret manager using static secrets.
    /// </summary>
    /// <param name="logger">The console logger service for error and diagnostic logging.</param>
    /// <param name="options">Configuration options containing database connection parameters and secret settings.</param>
    /// <param name="staticSecretManager">The static secret manager for retrieving passwords from secure storage.</param>
    /// <remarks>
    /// In StaticSecret mode, the provider will query the secret manager for passwords using the pattern:
    /// <c>{config.SecretName}-{role}</c> where role is Owner, Application, or Reader.
    /// The username comes from configuration and remains constant.
    /// </remarks>
    public CsProvider(
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderMsSqlConfig> options,
        IPvNugsStaticSecretManager staticSecretManager) : this(logger, options)
    {
        _staticSecretManager = staticSecretManager;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsProvider"/> class for DynamicSecret mode.
    /// Use this constructor when credentials should be dynamically generated with automatic expiration and renewal.
    /// </summary>
    /// <param name="logger">The console logger service for error and diagnostic logging.</param>
    /// <param name="options">Configuration options containing database connection parameters and secret settings.</param>
    /// <param name="dynamicSecretManager">The dynamic secret manager for generating temporary database credentials.</param>
    /// <remarks>
    /// In DynamicSecret mode, the provider will use the dynamic secret manager to generate username/password pairs
    /// with expiration times. Connection strings are automatically refreshed when credentials expire.
    /// </remarks>
    public CsProvider(
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderMsSqlConfig> options,
        IPvNugsDynamicSecretManager dynamicSecretManager) : this(logger, options)
    {
        _dynamicSecretManager = dynamicSecretManager;
    }

    /// <summary>
    /// Provides MsSQL connection strings with role-based access control and multiple credential management modes.
    /// This class supports three operational modes: Config (configuration-based), StaticSecret (secret manager with static secrets),
    /// and DynamicSecret (secret manager with time-limited credentials).
    /// Connection strings are cached per role and automatically refreshed when dynamic credentials expire.
    /// </summary>
    /// <remarks>
    /// <para><c>Constructor Selection:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Config Mode:</c> Use the primary constructor with logger and options only. Credentials are read from configuration.</description></item>
    /// <item><description><c>StaticSecret Mode:</c> Use the constructor with <see cref="IPvNugsStaticSecretManager"/>. Passwords are retrieved from a secret manager using static secrets.</description></item>
    /// <item><description><c>DynamicSecret Mode:</c> Use the constructor with <see cref="IPvNugsDynamicSecretManager"/>. Username/password pairs are dynamically generated with expiration times.</description></item>
    /// </list>
    /// <para><c>Secret Name Resolution:</c></para>
    /// <para>For StaticSecret and DynamicSecret modes, the secret name is constructed as: <c>{SecretName}-{Role}</c></para>
    /// <para>Where SecretName comes from configuration and Role is the SQL role (Owner, Application, or Reader).</para>
    /// <para>Example: If SecretName is "myapp-db" and role is "Application", the secret manager will query for "myapp-db-Application".</para>
    /// </remarks>
    /// <seealso cref="IPvNugsMsSqlCsProvider"/>
    public CsProvider(IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderMsSqlConfig> options)
    {
        _logger = logger;
        _configRows = options.Value.Rows??new List<PvNugsCsProviderMsSqlConfigRow>();
    }

    /// <summary>
    /// Asynchronously retrieves a MsSQL connection string for the specified database role.
    /// Connection strings are cached per role and automatically refreshed when credentials expire (DynamicSecret mode only).
    /// </summary>
    /// <param name="role">The database role to get the connection string for. Defaults to Reader for least-privilege access.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during credential retrieval.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the complete MsSQL connection string
    /// configured for the specified role, including server, database, credentials, schema, and other connection parameters.
    /// </returns>
    /// <exception cref="PvNugsCsProviderException">
    /// Thrown when credential retrieval fails, configuration is invalid, or required secret managers are not provisioned.
    /// The inner exception contains the original error details.
    /// </exception>
    /// <remarks>
    /// <para>This method implements double-checked locking per role to ensure thread-safe credential caching and refresh.</para>
    /// <para>For expired credentials (DynamicSecret mode), the method will automatically fetch fresh credentials before returning.</para>
    /// <para>The secret name used for credential retrieval follows the pattern: <c>{config.SecretName}-{role}</c></para>
    /// </remarks>
    public async Task<string> GetConnectionStringAsync(
        SqlRoleEnu role = SqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        return await GetConnectionStringAsync(DefaultCsName, role, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a MsSQL connection string for the specified database role.
    /// Connection strings are cached per role and automatically refreshed when credentials expire (DynamicSecret mode only).
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration row.</param>
    /// <param name="role">The database role to get the connection string for. Defaults to Reader for least-privilege access.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during credential retrieval.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the complete MsSQL connection string
    /// configured for the specified role, including server, database, credentials, schema, and other connection parameters.
    /// </returns>
    /// <exception cref="PvNugsCsProviderException">
    /// Thrown when credential retrieval fails, configuration is invalid, or required secret managers are not provisioned.
    /// The inner exception contains the original error details.
    /// </exception>
    /// <remarks>
    /// <para>This method implements double-checked locking per role to ensure thread-safe credential caching and refresh.</para>
    /// <para>For expired credentials (DynamicSecret mode), the method will automatically fetch fresh credentials before returning.</para>
    /// <para>The secret name used for credential retrieval follows the pattern: <c>{config.SecretName}-{role}</c></para>
    /// </remarks>
    public async Task<string> GetConnectionStringAsync(
        string connectionStringName,
        SqlRoleEnu role = SqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        var configRow = GetConfigRow(connectionStringName);
        
        var csEntryKey = $"{connectionStringName}-{role}";
        var entry = _csEntries.GetValueOrDefault(csEntryKey);
        if (entry is not null && !entry.IsExpired) return entry.ConnectionString;

        var gate = _locks.GetOrAdd(
            csEntryKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            entry = _csEntries.GetValueOrDefault(csEntryKey);
            if (entry is not null && !entry.IsExpired) return entry.ConnectionString;

            var fresh = 
                await FetchCredentials(configRow, role, cancellationToken)
                .ConfigureAwait(false);
            _csEntries[csEntryKey] = fresh;
            return fresh.ConnectionString;
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>
    /// Fetches database credentials based on the configured mode (Config, StaticSecret, or DynamicSecret).
    /// </summary>
    /// <param name="configRow">The configuration for a given connectionStringName</param>
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous credential fetch operation.</returns>
    private async Task<CsEntry> FetchCredentials(
        PvNugsCsProviderMsSqlConfigRow configRow,
        SqlRoleEnu role,
        CancellationToken cancellationToken = default)
    {
        if (configRow.UseIntegratedSecurity)
        {
            return 
                await FetchIntegratedSecurityCredentialsAsync(configRow, role);
        }

        await _logger.LogAsync(
            $"using SQL authentication for config '{configRow.Name}' " +
            $"with mode '{configRow.Mode}' " +
            $"and role '{role}'", SeverityEnu.Info);
        return configRow.Mode switch
        {
            CsProviderModeEnu.Config =>
                await FetchConfigCredentialsAsync(configRow, role),
            CsProviderModeEnu.StaticSecret =>
                await FetchStaticCredentialsAsync(configRow, role, cancellationToken),
            CsProviderModeEnu.DynamicSecret =>
                await FetchDynamicCredentialsAsync(configRow, role, cancellationToken),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<CsEntry> FetchIntegratedSecurityCredentialsAsync(
        PvNugsCsProviderMsSqlConfigRow configRow,
        SqlRoleEnu role)
    {
        // Windows Auth - username/password in config are optional/ignored
        await _logger.LogAsync(
            $"Using Windows Authentication for config '{configRow.Name}' and role '{role}'",
            SeverityEnu.Info);
        
        var cs = BuildConnectionString(
            configRow.Server, configRow.Database,
            true, role,
            configRow.Port,
            null, configRow.ApplicationName, null,
            configRow.TimeoutInSeconds);
        
        var effectiveUsername = GetCurrentUsername();
        
        return new CsEntry(
            effectiveUsername, cs, null);
    }
    
    /// <summary>
    /// Gets the current username in a cross-platform compatible format.
    /// </summary>
    /// <returns>Formatted username appropriate for the current operating system.</returns>
    private static string GetCurrentUsername()
    {
        var user = Environment.UserName;
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // Windows: Domain\Username or Machine\Username
                var domain = Environment.UserDomainName;
                return string.IsNullOrEmpty(domain) ? user : $"{domain}\\{user}";
            }
            else
            {
                // Linux/macOS: username@hostname
                var machine = Environment.MachineName;
                return $"{user}@{machine}";
            }
        }
        catch (Exception)
        {
            // Fallback to just username if anything fails
            return user;
        }
    }


    /// <summary>
    /// Fetches credentials from configuration settings (Config mode).
    /// Uses static username and password from the configuration file.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation with cached credentials.</returns>
    private async Task<CsEntry> FetchConfigCredentialsAsync(
        PvNugsCsProviderMsSqlConfigRow configRow,
        SqlRoleEnu role)
    {
        if (string.IsNullOrWhiteSpace(configRow.Username))
        {
            const string err = "Username is required when not using integrated security";
            await _logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        // Password is technically optional (could be empty password)
        if (string.IsNullOrEmpty(configRow.Password))
        {
            await _logger.LogAsync(
                $"No password specified for SQL authentication user: {configRow.Username}",
                SeverityEnu.Warning);
        }

        var username = configRow.Username!;
        var cs = BuildConnectionString(
            configRow.Server, configRow.Database,
            false, role,
            configRow.Port, username, configRow.ApplicationName,
            configRow.Password, configRow.TimeoutInSeconds);

        return new CsEntry(
            username, cs, null);
    }

    /// <summary>
    /// Fetches credentials from static secret manager (StaticSecret mode).
    /// Retrieves password from secret manager using the role-specific secret name pattern.
    /// </summary>
    /// <param name="configRow">The configuration for a given connectionStringName</param>
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation with cached credentials.</returns>
    private async Task<CsEntry> FetchStaticCredentialsAsync(
        PvNugsCsProviderMsSqlConfigRow configRow,
        SqlRoleEnu role,
        CancellationToken cancellationToken = default)
    {
        if (_staticSecretManager == null)
        {
            const string err = "StaticSecretManager has not been provisioned";
            await _logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        if (string.IsNullOrEmpty(configRow.Username))
        {
            const string err = "Username not found in configuration";
            await _logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }
        
        // Add validation for SecretName
        if (string.IsNullOrWhiteSpace(configRow.SecretName))
        {
            const string err = "SecretName not found in configuration for StaticSecret mode";
            await _logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }


        var username = configRow.Username;
        try
        {
            var secretName = $"{configRow.SecretName}-{role}";
            var password = await
                _staticSecretManager.GetStaticSecretAsync(
                    secretName, cancellationToken);
            if (password == null)
            {
                var err = $"Password not found for {secretName}";
                await _logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsCsProviderException(err);
            }

            var cs = BuildConnectionString(
                configRow.Server, configRow.Database,
                false, role,
                configRow.Port,
                username, configRow.ApplicationName,
                password,
                configRow.TimeoutInSeconds);

            return new CsEntry(
                username, cs, null);
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
    }

    /// <summary>
    /// Fetches dynamic credentials from secret manager (DynamicSecret mode).
    /// Retrieves temporary username/password pairs with expiration times using the role-specific secret name pattern.
    /// </summary>
    /// <param name="configRow">The configuration for a given connectionStringName</param>
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation with cached credentials including expiration.</returns>
    private async Task<CsEntry> FetchDynamicCredentialsAsync(
        PvNugsCsProviderMsSqlConfigRow configRow,
        SqlRoleEnu role,
        CancellationToken cancellationToken = default)
    {
        if (_dynamicSecretManager == null)
        {
            const string err = "DynamicSecretManager has not been provisioned";
            await _logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        // Add validation for SecretName
        if (string.IsNullOrWhiteSpace(configRow.SecretName))
        {
            const string err = "SecretName not found in configuration for DynamicSecret mode";
            await _logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        try
        {
            var secretName = $"{configRow.SecretName}-{role}";
            var dbSecret = await
                _dynamicSecretManager.GetDynamicSecretAsync(
                    secretName, cancellationToken);
            if (dbSecret == null)
            {
                var err = $"Dynamic Secret not found for {secretName}";
                await _logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsCsProviderException(err);
            }

            var username = dbSecret.Username;
            var cs = BuildConnectionString(
                configRow.Server, configRow.Database,
                false, role,
                configRow.Port, username, configRow.ApplicationName,
                dbSecret.Password,
                configRow.TimeoutInSeconds);
            var csExpirationDateUtc = dbSecret.ExpirationDateUtc;

            return new CsEntry(
                username, cs, csExpirationDateUtc);
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
    }

    /// <summary>
    /// Builds a SQL Server connection string with the specified parameters.
    /// Handles both SQL Server authentication and Windows Integrated Security.
    /// </summary>
    /// <param name="server">The database server hostname, IP address, or instance name (e.g., "server\instance").</param>
    /// <param name="database">The database name to connect to.</param>
    /// <param name="useTrustedConnection">True to use Windows Authentication, false for SQL Server authentication.</param>
    /// <param name="role">The SQL role context for application name suffix.</param>
    /// <param name="port">Optional port number. If not specified, uses SQL Server default or named instance.</param>
    /// <param name="username">Username for SQL Server authentication (ignored if using trusted connection).</param>
    /// <param name="applicationName">Application name for connection identification and monitoring.</param>
    /// <param name="password">Password for SQL Server authentication (ignored if using trusted connection).</param>
    /// <param name="timeoutInSeconds">Optional connection timeout in seconds.</param>
    /// <returns>A complete SQL Server connection string ready for use with SqlConnection.</returns>
    private static string BuildConnectionString(
        string server, string database,
        bool useTrustedConnection, SqlRoleEnu role,
        int? port,
        string? username, string? applicationName,
        string? password, int? timeoutInSeconds)
    {
        // Format the server/data source correctly for SQL Server
        var dataSource = FormatSqlServerDataSource(server, port);

        var cs = $"Server={dataSource};" +
                 $"Database={database};" +
                 $"TrustServerCertificate=true;"; // For development; configure appropriately for production

        // Windows Authentication
        if (useTrustedConnection)
        {
            cs += "Integrated Security=true;";
            // Note: username and password are ignored when using Windows Authentication

            // Application name with role suffix
            if (!string.IsNullOrWhiteSpace(applicationName))
            {
                cs += $"Application Name={applicationName}-{role};";
            }
        }
        else
        {
            cs += "Integrated Security=false;";

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required when not using integrated security",
                    nameof(username));

            cs += $"User ID={username};";

            if (!string.IsNullOrWhiteSpace(password))
                cs += $"Password={password};";
        }

        // Connection timeout
        if (timeoutInSeconds.HasValue)
            cs += $"Connection Timeout={timeoutInSeconds.Value};";


        return cs;
    }

    /// <summary>
    /// Formats the data source string for SQL Server, properly handling ports and named instances.
    /// </summary>
    /// <param name="server">Server name or IP address, potentially including instance name.</param>
    /// <param name="port">Optional port number.</param>
    /// <returns>Properly formatted data source string for SQL Server.</returns>
    private static string FormatSqlServerDataSource(string server, int? port)
    {
        // If no port specified, return server as-is (handles named instances like "server\SQLEXPRESS")
        if (!port.HasValue)
            return server;

        // If the server already contains a port (comma) or named instance (backslash), don't modify
        if (server.Contains(',') || server.Contains('\\'))
            return server;

        // Add port using SQL Server format: server,port
        return $"{server},{port.Value}";
    }
}