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
                _staticSecretManager!.GetStaticSecretAsync(
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
                _dynamicSecretManager!.GetDynamicSecretAsync(
                    secretName, cancellationToken);
            if (dbSecret == null)
            {
                var err = $"dbSecret not found for {_config.SecretName}";
                await logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsCsProviderException(err);
            }

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
    /// Builds a PostgreSQL connection string with the specified parameters.
    /// Automatically includes the configured schema in the Search Path.
    /// </summary>
    /// <param name="server">The database server hostname or IP address.</param>
    /// <param name="database">The database name to connect to.</param>
    /// <param name="port">Optional port number for the database connection.</param>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">Optional password for authentication.</param>
    /// <param name="timezone">Optional timezone setting for the connection.</param>
    /// <param name="timeoutInSeconds">Optional command timeout in seconds.</param>
    /// <returns>A complete PostgreSQL connection string ready for use with Npgsql.</returns>
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
                 "Include Error Detail=true";
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