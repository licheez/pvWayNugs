using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc10Abstractions;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsCsProviderNc10MsSql;

/// <summary>
/// Provides MsSQL connection strings with role-based access control and multiple credential management modes.
/// This class supports three operational modes: Config (configuration-based), StaticSecret (secret manager with static secrets),
/// and DynamicSecret (secret manager with time-limited credentials).
/// Connection strings are cached per role and automatically refreshed when dynamic credentials expire.
/// </summary>
/// <remarks>
/// <para><strong>Constructor Selection:</strong></para>
/// <list type="bullet">
/// <item><description><strong>Config Mode:</strong> Use the primary constructor with logger and options only. Credentials are read from configuration.</description></item>
/// <item><description><strong>StaticSecret Mode:</strong> Use the constructor with <see cref="IPvNugsSecretManager"/>. Passwords are retrieved from a secret manager using static secret APIs.</description></item>
/// <item><description><strong>DynamicSecret Mode:</strong> Use the constructor with <see cref="IPvNugsSecretManager"/> (must have SupportsDatabaseSecrets = true). Username/password pairs are dynamically generated with expiration times.</description></item>
/// </list>
/// <para><strong>Secret Name Resolution:</strong></para>
/// <para>For StaticSecret and DynamicSecret modes, the secret name is constructed as: <c>{SecretName}-{Role}</c></para>
/// <para>Where SecretName comes from configuration and Role is the SQL role (Owner, Application, or Reader).</para>
/// <para>Example: If SecretName is "myapp-db" and role is "Application", the secret manager will query for "myapp-db-Application".</para>
/// <para><strong>Expiration Management (Dynamic Mode):</strong></para>
/// <para>Dynamic credentials support configurable expiration tolerance thresholds:</para>
/// <list type="bullet">
/// <item><description><strong>Warning Threshold:</strong> Logs warnings when credentials are approaching expiration (default: 30 minutes before)</description></item>
/// <item><description><strong>Error Threshold:</strong> Throws exceptions when credentials are too close to expiration for safe use (default: 5 minutes before)</description></item>
/// </list>
/// <para>These thresholds can be configured via <see cref="PvNugsCsProviderMsSqlConfig.ExpirationWarningToleranceInMinutes"/> and <see cref="PvNugsCsProviderMsSqlConfig.ExpirationErrorToleranceInMinutes"/>.</para>
/// <para><strong>Thread Safety:</strong></para>
/// <para>This class is thread-safe and uses per-role semaphores to prevent concurrent credential fetching for the same role while allowing parallel access across different roles.</para>
/// <para><strong>Performance Characteristics:</strong></para>
/// <list type="bullet">
/// <item><description>O(1) cached connection string retrieval per role</description></item>
/// <item><description>Automatic cache invalidation for expired credentials</description></item>
/// <item><description>Double-checked locking pattern for thread-safe cache access</description></item>
/// <item><description>Minimal contention with role-specific synchronization</description></item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Config Mode Usage:</strong></para>
/// <code>
/// var services = new ServiceCollection();
/// services.Configure&lt;PvNugsCsProviderMsSqlConfig&gt;(config =&gt;
/// {
///     config.Mode = CsProviderModeEnu.Config;
///     config.Server = "localhost";
///     config.Database = "myapp";
///     config.Schema = "public";
///     config.Username = "app_user";
///     config.Password = "secure_password";
/// });
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLogger&gt;();
/// services.AddSingleton&lt;IPvNugsCsProvider, CsProvider&gt;();
///
/// var provider = serviceProvider.GetService&lt;IPvNugsCsProvider&gt;();
/// var connectionString = await provider.GetConnectionStringAsync(SqlRoleEnu.Reader);
/// </code>
///
/// <para><strong>DynamicSecret Mode Usage:</strong></para>
/// <code>
/// var services = new ServiceCollection();
/// services.Configure&lt;PvNugsCsProviderMsSqlConfig&gt;(config =&gt;
/// {
///     config.Mode = CsProviderModeEnu.DynamicSecret;
///     config.Server = "localhost";
///     config.Database = "myapp";
///     config.Schema = "public";
///     config.SecretName = "myapp-db";
///     config.ExpirationWarningToleranceInMinutes = 45;
///     config.ExpirationErrorToleranceInMinutes = 10;
/// });
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLogger&gt;();
/// services.AddSingleton&lt;IPvNugsSecretManager, VaultSecretManager&gt;(); // Must have SupportsDatabaseSecrets = true
/// services.AddSingleton&lt;IPvNugsCsProvider, CsProvider&gt;();
///
/// var provider = serviceProvider.GetService&lt;IPvNugsCsProvider&gt;();
/// var connectionString = await provider.GetConnectionStringAsync(SqlRoleEnu.Application);
/// // Credentials will be automatically refreshed before expiration
/// </code>
/// </example>
internal class CsProvider(
    IConsoleLoggerService logger,
    IOptions<PvNugsCsProviderMsSqlConfig> options) : IPvNugsMsSqlCsProvider
{
    private const string DefaultCsName = "Default";

    /// <summary>
    /// Represents a cached connection string entry with optional expiration support.
    /// Used internally to cache connection strings and track their validity for dynamic credentials.
    /// </summary>
    private sealed class CsEntry(
        string username,
        string connectionString,
        DateTime? expirationDateUtc)
    {
        /// <summary>
        /// Gets the username associated with this connection string entry.
        /// </summary>
        public string UserName { get; } = username;
        
        /// <summary>
        /// Gets the complete connection string for database connections.
        /// </summary>
        public string ConnectionString { get; } = connectionString;
        
        /// <summary>
        /// Gets the UTC expiration date for this entry, if applicable.
        /// </summary>
        private DateTime? ExpirationDateUtc { get; } = expirationDateUtc;

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

    private readonly IPvNugsSecretManager? _secretManager;

    private readonly ConcurrentDictionary<string, CsEntry> _csEntries = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private readonly IEnumerable<PvNugsCsProviderMsSqlConfigRow>
        _configRows = options.Value.Rows??[];
    
    private PvNugsCsProviderMsSqlConfigRow GetConfigRow(string connectionStringName)
    {
        var row = _configRows.FirstOrDefault(x =>
            string.Compare(x.Name, connectionStringName, StringComparison.InvariantCultureIgnoreCase) == 0);
        if (row != null) return row;
        var err = $"Connection string name '{connectionStringName}' not found in configuration";
        logger.Log(err, SeverityEnu.Error);
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
    public string GetUsername(CsProviderSqlRoleEnu role) => GetUsername(DefaultCsName, role);

    /// <summary>
    /// Gets the username for the specified connection string and database role from the current cache.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration row.</param>
    /// <param name="role">The database role to get the username for.</param>
    /// <returns>The cached username for the role, or an empty string if no cached entry exists.</returns>
    public string GetUsername(string connectionStringName, CsProviderSqlRoleEnu role)
    {
        var csEntryKey = $"{connectionStringName}-{role}";
        var exists = _csEntries.TryGetValue(csEntryKey, out var csEntry);
        return exists ? csEntry!.UserName : string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsProvider"/> class for StaticSecret or DynamicSecret mode.
    /// Use this constructor when credentials should be retrieved from a secret manager.
    /// </summary>
    /// <param name="logger">The console logger service for error and diagnostic logging.</param>
    /// <param name="options">Configuration options containing database connection parameters and secret settings.</param>
    /// <param name="secretManager">The secret manager for retrieving credentials from secure storage. Must have SupportsDatabaseSecrets = true for DynamicSecret mode.</param>
    /// <remarks>
    /// <para>The provider's behavior depends on the configured mode:</para>
    /// <list type="bullet">
    /// <item><description><strong>StaticSecret mode:</strong> Uses GetStaticSecretAsync to retrieve passwords. The username comes from configuration and remains constant. Secret names follow the pattern: <c>{config.SecretName}-{role}</c></description></item>
    /// <item><description><strong>DynamicSecret mode:</strong> Uses GetDynamicSecretAsync to retrieve username/password pairs with expiration times. Requires secretManager.SupportsDatabaseSecrets = true. Secret names follow the pattern: <c>{config.SecretName}-{role}</c></description></item>
    /// </list>
    /// </remarks>
    public CsProvider(
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderMsSqlConfig> options,
        IPvNugsSecretManager secretManager) : this(logger, options)
    {
        _secretManager = secretManager;
    }

    /// <summary>
    /// Asynchronously retrieves a PostgreSQL connection string for the specified database role from the default configuration row.
    /// </summary>
    /// <param name="role">The database role to get the connection string for. Defaults to Reader for least-privilege access.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during credential retrieval.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the complete PostgreSQL connection string for the specified role.</returns>
    public async Task<string> GetConnectionStringAsync(
        CsProviderSqlRoleEnu role = CsProviderSqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        return await GetConnectionStringAsync(DefaultCsName, role, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a PostgreSQL connection string for the specified database role and configuration row.
    /// </summary>
    /// <param name="connectionStringName">The name of the configuration row to use.</param>
    /// <param name="role">The database role to get the connection string for. Defaults to Reader for least-privilege access.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during credential retrieval.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the complete PostgreSQL connection string for the specified role and configuration row.</returns>
    public async Task<string> GetConnectionStringAsync(
        string connectionStringName,
        CsProviderSqlRoleEnu role = CsProviderSqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        var configRow = GetConfigRow(connectionStringName);
        
        var csEntryKey = $"{connectionStringName}-{role}";
        var entry = _csEntries.GetValueOrDefault(csEntryKey);
        if (entry is not null && !entry.IsExpired) 
            return entry.ConnectionString;

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
            await logger.LogAsync(e);
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
    /// <param name="configRow">The configuration row to use for credential retrieval.</param>
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous credential fetch operation.</returns>
    private async Task<CsEntry> FetchCredentials(
        PvNugsCsProviderMsSqlConfigRow configRow,
        CsProviderSqlRoleEnu role,
        CancellationToken cancellationToken = default)
    {
        if (configRow.UseIntegratedSecurity)
        {
            return 
                await FetchIntegratedSecurityCredentialsAsync(configRow, role);
        }
        
        await logger.LogAsync(
            $"using SQL authentication for config '{configRow.Name}' " +
            $"with mode '{configRow.Mode}' " +
            $"and role '{role}'", SeverityEnu.Info);

        return configRow.Mode switch
        {
            CsProviderModeEnu.Config =>
                await FetchConfigCredentialsAsync(configRow, role),
            CsProviderModeEnu.StaticSecret =>
                await FetchStaticCredentialsAsync(
                    configRow, role, cancellationToken),
            CsProviderModeEnu.DynamicSecret =>
                await FetchDynamicCredentialsAsync(
                    configRow, role, cancellationToken),
            _ => throw new SwitchExpressionException()
        };
    }
    
    private async Task<CsEntry> FetchIntegratedSecurityCredentialsAsync(
        PvNugsCsProviderMsSqlConfigRow configRow,
        CsProviderSqlRoleEnu role)
    {
        // Windows Auth - username/password in config are optional/ignored
        await logger.LogAsync(
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
        CsProviderSqlRoleEnu role)
    {
        if (string.IsNullOrWhiteSpace(configRow.Username))
        {
            const string err = "Username is required when not using integrated security";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        // Password is technically optional (could be empty password)
        if (string.IsNullOrEmpty(configRow.Password))
        {
            await logger.LogAsync(
                $"No password specified for user: {configRow.Username}",
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
    /// </summary>
    /// <param name="configRow">The configuration row to use for credential retrieval.</param>
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation with cached credentials.</returns>
    private async Task<CsEntry> FetchStaticCredentialsAsync(
        PvNugsCsProviderMsSqlConfigRow configRow,
        CsProviderSqlRoleEnu role,
        CancellationToken cancellationToken)
    {
        if (_secretManager == null)
        {
            const string err = "SecretManager has not been provisioned";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        if (string.IsNullOrEmpty(configRow.Username))
        {
            const string err = "Username not found in configuration";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        var username = configRow.Username;
        try
        {
            var sParams = GetSecretParamsForRole(
                configRow, role);
            var password = await
                _secretManager.GetStaticSecretAsync(
                    sParams, cancellationToken);
            if (password == null)
            {
                var err = $"password not found for role {role}";
                await logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsCsProviderException(err);
            }
            
            var cs = BuildConnectionString(
                configRow.Server, configRow.Database, 
                false, role, 
                configRow.Port, 
                username, configRow.ApplicationName,
                password, configRow.TimeoutInSeconds);

            return new CsEntry(
                username, cs, null);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
    }

    /// <summary>
    /// Fetches dynamic credentials from secret manager (DynamicSecret mode).
    /// </summary>
    /// <param name="configRow">The configuration row to use for credential retrieval.</param>
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation with cached credentials including expiration.</returns>
    private async Task<CsEntry> FetchDynamicCredentialsAsync(
        PvNugsCsProviderMsSqlConfigRow configRow,
        CsProviderSqlRoleEnu role,
        CancellationToken cancellationToken)
    {
        if (_secretManager == null)
        {
            const string err = "SecretManager has not been provisioned";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        try
        {
            var sParams = GetSecretParamsForRole(
                configRow, role);
            var dbSecret = await
                _secretManager.GetDynamicSecretAsync(
                    sParams, cancellationToken);
            if (dbSecret == null)
            {
                var err = $"dbSecret not found for role {role}";
                await logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsCsProviderException(err);
            }
            
            await ValidateSecretExpirationAsync(configRow, dbSecret, role);
            
            var username = dbSecret.Username;
            
            var cs = BuildConnectionString(
                configRow.Server, configRow.Database,
                false, role,
                configRow.Port, username, 
                configRow.ApplicationName, dbSecret.Password, 
                configRow.TimeoutInSeconds);
            var csExpirationDateUtc = dbSecret.ExpirationDateUtc;

            return new CsEntry(
                username, cs, csExpirationDateUtc);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
    }
    
    /// <summary>
    /// Validates the expiration of a database secret with configurable tolerance thresholds.
    /// </summary>
    /// <param name="dbSecret">The database secret to validate containing expiration information.</param>
    /// <param name="configRow">The configuration row to use for tolerance settings.</param>
    /// <param name="role">The role associated with the secret for logging purposes.</param>
    /// <returns>A task representing the asynchronous validation operation.</returns>
    private async Task ValidateSecretExpirationAsync(
        PvNugsCsProviderMsSqlConfigRow configRow,
        IPvNugsDynamicCredential dbSecret,
        CsProviderSqlRoleEnu role)
    {
        var expirationDateUtc = dbSecret.ExpirationDateUtc;
        var currentUtc = DateTime.UtcNow;
        
        // Configuration for tolerance - could be moved to config if needed
        var warningToleranceMinutes = configRow.ExpirationWarningToleranceInMinutes ?? 30; // Default 30 minutes
        var errorToleranceMinutes = configRow.ExpirationErrorToleranceInMinutes ?? 5; // Default 5 minutes
        
        var warningThreshold = expirationDateUtc.AddMinutes(-warningToleranceMinutes);
        var errorThreshold = expirationDateUtc.AddMinutes(-errorToleranceMinutes);

        if (currentUtc >= expirationDateUtc)
        {
            var err = $"Secret for role '{role}' has expired at {expirationDateUtc:yyyy-MM-dd HH:mm:ss} UTC";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }
        
        if (currentUtc >= errorThreshold)
        {
            var timeRemaining = expirationDateUtc - currentUtc;
            var err = $"Secret for role '{role}' will expire in {timeRemaining.TotalMinutes:F1} " +
                      $"minutes at {expirationDateUtc:yyyy-MM-dd HH:mm:ss} UTC";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }
        
        if (currentUtc >= warningThreshold)
        {
            var timeRemaining = expirationDateUtc - currentUtc;
            var warning = $"Secret for role '{role}' will expire in {timeRemaining.TotalMinutes:F1} " +
                          $"minutes at {expirationDateUtc:yyyy-MM-dd HH:mm:ss} UTC";
            await logger.LogAsync(warning, SeverityEnu.Warning);
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
        bool useTrustedConnection, CsProviderSqlRoleEnu role,
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
    
    private static Dictionary<string, string> GetSecretParamsForRole(
        PvNugsCsProviderMsSqlConfigRow configRow, CsProviderSqlRoleEnu role)
    {
        return role switch
        {
            CsProviderSqlRoleEnu.Owner => configRow.OwnerSecretParams ?? new Dictionary<string, string>(),
            CsProviderSqlRoleEnu.Application => configRow.ApplicationSecretParams ?? new Dictionary<string, string>(),
            CsProviderSqlRoleEnu.Reader => configRow.ReaderSecretParams ?? new Dictionary<string, string>(),
            _ => throw new SwitchExpressionException()
        };
    }
}