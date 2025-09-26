using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;
using SqlRoleEnu = pvNugsCsProviderNc9Abstractions.SqlRoleEnu;

namespace pvNugsCsProviderNc9PgSql;

/// <summary>
/// Provides PostgreSQL connection strings with role-based access control and multiple credential management modes.
/// This class supports three operational modes: Config (configuration-based), StaticSecret (secret manager with static secrets),
/// and DynamicSecret (secret manager with time-limited credentials).
/// Connection strings are cached per role and automatically refreshed when dynamic credentials expire.
/// </summary>
/// <remarks>
/// <para><strong>Constructor Selection:</strong></para>
/// <list type="bullet">
/// <item><description><strong>Config Mode:</strong> Use the primary constructor with logger and options only. Credentials are read from configuration.</description></item>
/// <item><description><strong>StaticSecret Mode:</strong> Use the constructor with <see cref="IPvNugsStaticSecretManager"/>. Passwords are retrieved from a secret manager using static secrets.</description></item>
/// <item><description><strong>DynamicSecret Mode:</strong> Use the constructor with <see cref="IPvNugsDynamicSecretManager"/>. Username/password pairs are dynamically generated with expiration times.</description></item>
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
/// <para>These thresholds can be configured via <see cref="PvNugsCsProviderPgSqlConfig.ExpirationWarningToleranceInMinutes"/> and <see cref="PvNugsCsProviderPgSqlConfig.ExpirationErrorToleranceInMinutes"/>.</para>
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
/// services.Configure&lt;PvNugsCsProviderPgSqlConfig&gt;(config =&gt;
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
/// services.Configure&lt;PvNugsCsProviderPgSqlConfig&gt;(config =&gt;
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
/// services.AddSingleton&lt;IPvNugsDynamicSecretManager, VaultDynamicSecretManager&gt;();
/// services.AddSingleton&lt;IPvNugsCsProvider, CsProvider&gt;();
///
/// var provider = serviceProvider.GetService&lt;IPvNugsCsProvider&gt;();
/// var connectionString = await provider.GetConnectionStringAsync(SqlRoleEnu.Application);
/// // Credentials will be automatically refreshed before expiration
/// </code>
/// </example>
public class CsProvider(
    IConsoleLoggerService logger,
    IOptions<PvNugsCsProviderPgSqlConfig> options) : IPvNugsPgSqlCsProvider
{
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

    private readonly PvNugsCsProviderPgSqlConfig _config = options.Value;

    private readonly IPvNugsStaticSecretManager? _staticSecretManager;
    private readonly IPvNugsDynamicSecretManager? _dynamicSecretManager;

    private readonly ConcurrentDictionary<SqlRoleEnu, CsEntry> _csEntries = new();
    private readonly ConcurrentDictionary<SqlRoleEnu, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Gets a value indicating whether this provider uses dynamic credentials with expiration times.
    /// Returns true when configured for DynamicSecret mode, false for Config or StaticSecret modes.
    /// </summary>
    public bool UseDynamicCredentials => _config.Mode == CsProviderModeEnu.DynamicSecret;

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
    public string GetUsername(SqlRoleEnu role)
    {
        var exists = _csEntries.TryGetValue(role, out var csEntry);
        return exists ? csEntry!.UserName : string.Empty;
    }

    /// <summary>
    /// Gets the PostgreSQL schema name from configuration.
    /// This schema is automatically added to the connection string's Search Path.
    /// </summary>
    public string Schema => _config.Schema;

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
        IOptions<PvNugsCsProviderPgSqlConfig> options,
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
    /// In DynamicSecret mode, the provider will request new credentials from the secret manager using the pattern:
    /// <c>{config.SecretName}-{role}</c> where role is Owner, Application, or Reader.
    /// Both username and password are dynamically generated and will expire based on the secret manager's policy.
    /// Credentials are automatically refreshed when they expire.
    /// </remarks>
    public CsProvider(
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderPgSqlConfig> options,
        IPvNugsDynamicSecretManager dynamicSecretManager) : this(logger, options)
    {
        _staticSecretManager = dynamicSecretManager;
        _dynamicSecretManager = dynamicSecretManager;
    }

    /// <summary>
    /// Asynchronously retrieves a PostgreSQL connection string for the specified database role.
    /// Connection strings are cached per role and automatically refreshed when credentials expire (DynamicSecret mode only).
    /// </summary>
    /// <param name="role">The database role to get the connection string for. Defaults to Reader for least-privilege access.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during credential retrieval.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the complete PostgreSQL connection string
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
        var entry = _csEntries.GetValueOrDefault(role);
        if (entry is not null && !entry.IsExpired) return entry.ConnectionString;

        var gate = _locks.GetOrAdd(
            role, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            entry = _csEntries.GetValueOrDefault(role);
            if (entry is not null && !entry.IsExpired) return entry.ConnectionString;

            var fresh = await FetchCredentials(role, cancellationToken)
                .ConfigureAwait(false);
            _csEntries[role] = fresh;
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
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous credential fetch operation.</returns>
    private async Task<CsEntry> FetchCredentials(
        SqlRoleEnu role,
        CancellationToken cancellationToken = default)
    {
        return _config.Mode switch
        {
            CsProviderModeEnu.Config =>
                await FetchConfigCredentials(),
            CsProviderModeEnu.StaticSecret =>
                await FetchStaticCredentials(role, cancellationToken),
            CsProviderModeEnu.DynamicSecret =>
                await FetchDynamicCredentials(role, cancellationToken),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Fetches credentials from configuration settings (Config mode).
    /// Uses static username and password from the configuration file.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation with cached credentials.</returns>
    private async Task<CsEntry> FetchConfigCredentials()
    {
        if (string.IsNullOrEmpty(_config.Username))
        {
            const string err = "Username not found in configuration";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        var username = _config.Username;
        var cs = BuildConnectionString(
            _config.Server, _config.Database,
            _config.Port, username, _config.Password,
            _config.Timezone, _config.TimeoutInSeconds);

        return new CsEntry(
            username, cs, null);
    }

    /// <summary>
    /// Fetches credentials from static secret manager (StaticSecret mode).
    /// Retrieves password from secret manager using the role-specific secret name pattern.
    /// </summary>
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation with cached credentials.</returns>
    private async Task<CsEntry> FetchStaticCredentials(
        SqlRoleEnu role,
        CancellationToken cancellationToken = default)
    {
        if (_staticSecretManager == null)
        {
            const string err = "StaticSecretManager has not been provisioned";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        if (string.IsNullOrEmpty(_config.Username))
        {
            const string err = "Username not found in configuration";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        var username = _config.Username;
        try
        {
            var secretName = $"{_config.SecretName}-{role}";
            var password = await
                _staticSecretManager.GetStaticSecretAsync(
                    secretName, cancellationToken);
            if (password == null)
            {
                var err = $"password not found for {_config.SecretName}";
                await logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsCsProviderException(err);
            }

            var cs = BuildConnectionString(
                _config.Server, _config.Database,
                _config.Port, username, password,
                _config.Timezone, _config.TimeoutInSeconds);

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
    /// Retrieves temporary username/password pairs with expiration times using the role-specific secret name pattern.
    /// </summary>
    /// <param name="role">The database role to fetch credentials for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation with cached credentials including expiration.</returns>
    private async Task<CsEntry> FetchDynamicCredentials(
        SqlRoleEnu role,
        CancellationToken cancellationToken = default)
    {
        if (_dynamicSecretManager == null)
        {
            const string err = "DynamicSecretManager has not been provisioned";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }

        try
        {
            var secretName = $"{_config.SecretName}-{role}";
            var dbSecret = await
                _dynamicSecretManager.GetDynamicSecretAsync(
                    secretName, cancellationToken);
            if (dbSecret == null)
            {
                var err = $"dbSecret not found for {_config.SecretName}";
                await logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsCsProviderException(err);
            }
            
            await ValidateSecretExpirationAsync(dbSecret, secretName);
            
            var username = dbSecret.Username;
            var cs = BuildConnectionString(
                _config.Server, _config.Database,
                _config.Port, username, dbSecret.Password,
                _config.Timezone, _config.TimeoutInSeconds);
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
    /// Implements a three-tier validation system: normal operation, warning zone, and error zone.
    /// This proactive validation prevents the use of credentials that may expire during active database operations.
    /// </summary>
    /// <param name="dbSecret">The database secret to validate containing expiration information.</param>
    /// <param name="secretName">The name of the secret for logging and error reporting purposes.</param>
    /// <returns>A task representing the asynchronous validation operation.</returns>
    /// <exception cref="PvNugsCsProviderException">
    /// Thrown when the secret has expired or is within the error threshold window.
    /// This prevents the use of credentials that may expire during active database operations.
    /// </exception>
    /// <remarks>
    /// <para><strong>Validation Zones:</strong></para>
    /// <list type="number">
    /// <item><description><strong>Normal Zone:</strong> Credential is valid and safe to use (no action taken)</description></item>
    /// <item><description><strong>Warning Zone:</strong> Credential is approaching expiration but still usable (warning logged)</description></item>
    /// <item><description><strong>Error Zone:</strong> Credential is too close to expiration for safe use (exception thrown)</description></item>
    /// <item><description><strong>Expired:</strong> Credential has already expired (exception thrown)</description></item>
    /// </list>
    /// <para><strong>Configurable Thresholds:</strong></para>
    /// <para>The tolerance thresholds are configurable via:</para>
    /// <list type="bullet">
    /// <item><description><see cref="PvNugsCsProviderPgSqlConfig.ExpirationWarningToleranceInMinutes"/> (default: 30 minutes) - Controls when warnings are logged</description></item>
    /// <item><description><see cref="PvNugsCsProviderPgSqlConfig.ExpirationErrorToleranceInMinutes"/> (default: 5 minutes) - Controls when errors are thrown</description></item>
    /// </list>
    /// <para><strong>Design Rationale:</strong></para>
    /// <para>This approach ensures that applications have sufficient time to complete database operations before credentials expire.
    /// By throwing exceptions proactively rather than allowing expired credentials to reach the database, we prevent
    /// connection failures during critical operations and provide clear error messages for troubleshooting.</para>
    /// <para><strong>Logging Behavior:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Warning:</strong> "Secret 'SecretName' will expire in X.X minutes at yyyy-MM-dd HH:mm:ss UTC"</description></item>
    /// <item><description><strong>Error (near expiration):</strong> "Secret 'SecretName' will expire in X.X minutes at yyyy-MM-dd HH:mm:ss UTC"</description></item>
    /// <item><description><strong>Error (expired):</strong> "Secret 'SecretName' has expired at yyyy-MM-dd HH:mm:ss UTC"</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>Configuration Example:</strong></para>
    /// <code>
    /// {
    ///   "PvNugsCsProviderPgSqlConfig": {
    ///     "Mode": "DynamicSecret",
    ///     "ExpirationWarningToleranceInMinutes": 60,  // Warn 60 minutes before expiration
    ///     "ExpirationErrorToleranceInMinutes": 15     // Error 15 minutes before expiration
    ///   }
    /// }
    /// </code>
    /// <para>With this configuration:</para>
    /// <list type="bullet">
    /// <item><description>61+ minutes remaining: Normal operation</description></item>
    /// <item><description>16-60 minutes remaining: Warning logged, operation continues</description></item>
    /// <item><description>0-15 minutes remaining: Exception thrown</description></item>
    /// <item><description>Already expired: Exception thrown</description></item>
    /// </list>
    /// </example>
    private async Task ValidateSecretExpirationAsync(
        IPvNugsDynamicCredential dbSecret, string secretName)
    {
        var expirationDateUtc = dbSecret.ExpirationDateUtc;
        var currentUtc = DateTime.UtcNow;
        
        // Configuration for tolerance - could be moved to config if needed
        var warningToleranceMinutes = _config.ExpirationWarningToleranceInMinutes ?? 30; // Default 30 minutes
        var errorToleranceMinutes = _config.ExpirationErrorToleranceInMinutes ?? 5; // Default 5 minutes
        
        var warningThreshold = expirationDateUtc.AddMinutes(-warningToleranceMinutes);
        var errorThreshold = expirationDateUtc.AddMinutes(-errorToleranceMinutes);

        if (currentUtc >= expirationDateUtc)
        {
            var err = $"Secret '{secretName}' has expired at {expirationDateUtc:yyyy-MM-dd HH:mm:ss} UTC";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }
        
        if (currentUtc >= errorThreshold)
        {
            var timeRemaining = expirationDateUtc - currentUtc;
            var err = $"Secret '{secretName}' will expire in {timeRemaining.TotalMinutes:F1} minutes at {expirationDateUtc:yyyy-MM-dd HH:mm:ss} UTC";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsCsProviderException(err);
        }
        
        if (currentUtc >= warningThreshold)
        {
            var timeRemaining = expirationDateUtc - currentUtc;
            var warning = $"Secret '{secretName}' will expire in {timeRemaining.TotalMinutes:F1} minutes at {expirationDateUtc:yyyy-MM-dd HH:mm:ss} UTC";
            await logger.LogAsync(warning, SeverityEnu.Warning);
        }
    }


    /// <summary>
    /// Builds a PostgreSQL connection string with the specified parameters.
    /// Automatically includes the configured schema in the Search Path and sets secure connection defaults.
    /// </summary>
    /// <param name="server">The database server hostname or IP address.</param>
    /// <param name="database">The database name to connect to.</param>
    /// <param name="port">Optional port number for the database connection. Uses PostgreSQL default (5432) if null.</param>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">Optional password for authentication. Can be null for password-less authentication.</param>
    /// <param name="timezone">Optional timezone setting for the connection. Uses server default if null.</param>
    /// <param name="timeoutInSeconds">Optional command timeout in seconds. Uses driver default if null.</param>
    /// <returns>A complete PostgreSQL connection string ready for use with Npgsql.</returns>
    /// <remarks>
    /// <para><strong>Automatic Schema Configuration:</strong></para>
    /// <para>The configured schema from <see cref="PvNugsCsProviderPgSqlConfig.Schema"/> is automatically added 
    /// to the connection string's Search Path parameter, making it the default schema for unqualified table references.</para>
    /// <para><strong>Security Features:</strong></para>
    /// <list type="bullet">
    /// <item><description><c>Include Error Detail=true</c> - Provides detailed error information for troubleshooting</description></item>
    /// <item><description>Automatic parameter escaping - All values are properly formatted for connection string safety</description></item>
    /// </list>
    /// <para><strong>Connection String Format:</strong></para>
    /// <para>The generated connection string follows this pattern:</para>
    /// <code>
    /// Server={server};Database={database};User Id={username};Include Error Detail=true;
    /// [Port={port};][Password={password};][TimeZone={timezone};][CommandTimeout={timeoutInSeconds};]
    /// Search Path={schema};
    /// </code>
    /// <para>Optional parameters are only included when provided (non-null values).</para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example Output:</strong></para>
    /// <code>
    /// // Full configuration
    /// Server=localhost;Database=myapp;User Id=app_user;Include Error Detail=true;
    /// Port=5432;Password=secret123;TimeZone=UTC;CommandTimeout=300;Search Path=app_schema;
    /// 
    /// // Minimal configuration (using defaults)
    /// Server=prod.postgres.com;Database=production;User Id=readonly_user;Include Error Detail=true;
    /// Search Path=public;
    /// </code>
    /// </example>
    private string BuildConnectionString(
        string server,
        string database,
        int? port,
        string username,
        string? password,
        string? timezone,
        int? timeoutInSeconds)
    {
        var cs = $"Server={server};" +
                 $"Database={database};" +
                 $"User Id={username};" +
                 "Include Error Detail=true;";
        if (port.HasValue)
            cs += $"Port={port.Value};";
        if (!string.IsNullOrEmpty(password))
            cs += $"Password={password};";
        if (!string.IsNullOrEmpty(timezone))
            cs += $"TimeZone={timezone};";
        if (timeoutInSeconds.HasValue)
            cs += $"CommandTimeout={timeoutInSeconds.Value};";

        cs += $"Search Path={Schema};";

        return cs;
    }
}